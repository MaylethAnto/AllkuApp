using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AllkuApp.Modelo;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Xamarin.Essentials;
using AllkuApp.Dtos;
using TuProyecto.Models;

namespace AllkuApp.Servicios
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl = "https://allkuapi.sytes.net/api/Canino";


        public ApiService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Métodos para Caninos
        public async Task<List<CaninoDto>> GetCaninesAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Response JSON: {json}"); // Mensaje de depuración
                    return JsonConvert.DeserializeObject<List<CaninoDto>>(json);
                }
                else
                {
                    Debug.WriteLine($"Error: {response.ReasonPhrase}"); // Mensaje de depuración
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        //metodo obtener canino por id



        // Método para obtener los datos del dueño por cédula

        public async Task<Dueno> ObtenerDuenoPorCedulaAsync(string cedula)
        {
            try
            {
                var response = await _client.GetAsync($"https://allkuapi.sytes.net/api/Canino/usuarios?cedulaDueno={cedula}");

                if (response.IsSuccessStatusCode)
                {
                    // Lee la respuesta como un string
                    var content = await response.Content.ReadAsStringAsync();

                    // Deserializa el JSON en un objeto Dueno
                    var dueno = JsonConvert.DeserializeObject<Dueno>(content);
                    return dueno; // Retorna los datos del dueño si se encontró
                }
                else
                {
                    return null; // Si no se encuentra el dueño, retorna null
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
                return null;
            }
        }

        //metodo para obtener los caninos por medio de la cedula del dueño
        // Método para obtener los caninos por cédula del dueño
        public async Task<List<CaninoDto>> GetCaninosByCedulaDuenoAsync(string cedulaDueno)
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/caninosPorCedula?cedulaDueno={cedulaDueno}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Response JSON: {json}"); // Mensaje de depuración
                    return JsonConvert.DeserializeObject<List<CaninoDto>>(json);
                }
                else
                {
                    Debug.WriteLine($"Error: {response.ReasonPhrase}"); // Mensaje de depuración
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public async Task<string> RegistrarCaninoAsync(CaninoRequest caninoRequest)
        {
            try
            {
                // Supongamos que tienes una URL base y un endpoint para registrar la mascota
                string url = $"{_baseUrl}/RegistrarCanino";

                // Serializar el objeto canino a JSON
                var json = JsonConvert.SerializeObject(caninoRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Hacer una solicitud POST al API
                var response = await _client.PostAsync(url, content);

                // Leer la respuesta del API
                var responseContent = await response.Content.ReadAsStringAsync();

                // Verificar el estado de la respuesta
                if (response.IsSuccessStatusCode)
                {
                    return "Success";
                }
                else
                {
                    Debug.WriteLine($"Error en la respuesta del API: {responseContent}");
                    return "Error";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en RegistrarCaninoAsync: {ex.Message}");
                return "Exception";
            }
        }

        //metodos para notificaciones 
        public async Task<(bool, string)> EnviarNotificacionAsync(int idCanino, string mensaje, string numeroPaseador)
        {
            var notificacionRequest = new
            {
                IdCanino = idCanino,
                Mensaje = mensaje,
                NumeroPaseador = numeroPaseador
            };

            var json = JsonConvert.SerializeObject(notificacionRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.PostAsync("https://allkuapi.sytes.net/api/Notificacion/enviar", content);

                if (!response.IsSuccessStatusCode)
                {
                    // Capturar el contenido de la respuesta de error
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Error al enviar la notificación: {responseContent}");
                }

                return (true, "Notificación enviada exitosamente.");
            }
            catch (HttpRequestException httpEx)
            {
                // Capturar errores de solicitud HTTP
                return (false, $"Error de solicitud HTTP: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                // Capturar cualquier otro tipo de error
                return (false, $"Error: {ex.Message}");
            }
        }


        public async Task<bool> CheckForNotificationsAsync()
        {
            var cedulaDueno = Preferences.Get("CedulaDueno", string.Empty);
            var response = await _client.GetAsync($"https://allkuapi.sytes.net/api/Notificacion/check?cedulaDueno={cedulaDueno}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<int>(json) > 0;
            }
            return false;
        }

        public async Task<NotificacionDto> GetLatestNotificationAsync()
        {
            var cedulaDueno = Preferences.Get("CedulaDueno", string.Empty);
            var response = await _client.GetAsync($"https://allkuapi.sytes.net/api/Notificacion/ultima?cedulaDueno={cedulaDueno}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<NotificacionDto>(json);
            }
            return null;
        }

        public async Task MarcarNotificacionComoLeidaAsync(int idNotificacion)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new
            {
                idNotificacion = idNotificacion
            }), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"https://allkuapi.sytes.net/api/Notificacion/marcarComoLeida", content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error al marcar la notificación como leída: {response.ReasonPhrase}");
            }
        }



        // Métodos para Historial Clínico
        public async Task<List<Historial_Clinico>> GetHistorialClinicoAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}historialClinico");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Historial_Clinico>>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Historial_Clinico> AddHistorialClinicoAsync(Historial_Clinico historial)
        {
            try
            {
                var json = JsonConvert.SerializeObject(historial);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}historialClinico", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Historial_Clinico>(jsonResponse);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
        // Métodos para Ejercicio

        public async Task<DistanciaRecorrida> ObtenerDistanciaRecorridaAsync(int idCanino, DateTime fechaInicio, DateTime fechaFin)
        {
            var url = $"https://allkuapi.sytes.net/api/Gps/distancia?id_canino={idCanino}&fecha_inicio={fechaInicio:yyyy-MM-dd}&fecha_fin={fechaFin:yyyy-MM-dd}";

            try
            {
                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Respuesta API: {content}");

                    if (!string.IsNullOrWhiteSpace(content) && content != "null")
                    {
                        var distancia = JsonSerializer.Deserialize<DistanciaRecorrida>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        return distancia;
                    }
                    else
                    {
                        Debug.WriteLine("El contenido de la respuesta está vacío o es 'null'.");
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine($"Error en la respuesta: {response.StatusCode} - {response.ReasonPhrase}");
                    throw new Exception("Error al obtener la distancia recorrida");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ObtenerDistanciaRecorridaAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<DistanciaRecorrida> ObtenerHistorialRecorridosAsync()
        {
            try
            {
                // Llamar a la API y obtener la respuesta JSON
                var response = await _client.GetStringAsync("https://allkuapi.sytes.net/api/Gps/distancia?id_canino={idCanino}&fecha_inicio={fechaInicio:yyyy-MM-dd}&fecha_fin={fechaFin:yyyy-MM-dd}");

                // Deserializar a un único objeto, no a una lista
                var historial = JsonSerializer.Deserialize<DistanciaRecorrida>(response);

                return historial;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ObtenerHistorialRecorridosAsync: {ex.Message}");
                return null;
            }
        }




        // Métodos para GPS
        public async Task<List<Gps>> GetGpsAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}gps");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Gps>>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Gps> AddGpsAsync(Gps gps)
        {
            try
            {
                var json = JsonConvert.SerializeObject(gps);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}gps", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Gps>(jsonResponse);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        // Métodos para Manejo de Perfiles
        public async Task<List<Manejo_Perfiles>> GetPerfilesAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}perfiles");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Manejo_Perfiles>>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Manejo_Perfiles> AddPerfilAsync(Manejo_Perfiles perfil)
        {
            try
            {
                var json = JsonConvert.SerializeObject(perfil);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}perfiles", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Manejo_Perfiles>(jsonResponse);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }



        // Métodos para Receta
        public async Task<List<Receta>> GetRecetasAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}recetas");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Receta>>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Receta> AddRecetaAsync(Receta receta)
        {
            try
            {
                var json = JsonConvert.SerializeObject(receta);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}recetas", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Receta>(jsonResponse);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> PostAsync<T>(string endpoint, T data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }

        internal static async Task<IEnumerable<object>> ObtenerCaninosConDuenosAsync()
        {
            throw new NotImplementedException();
        }


    }

    public class CaninoRequest
    {
        public string NombreCanino { get; set; }
        public int EdadCanino { get; set; }
        public string RazaCanino { get; set; }
        public decimal PesoCanino { get; set; }
        public byte[] FotoCanino { get; set; }
        public string CedulaDueno { get; set; }
    }

    public class DistanciaRecorrida
    {
        public int Id { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public decimal DistanciaTotal { get; set; }
    }

    public class HistorialRecorrido
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public double DistanciaTotal { get; set; }
    }

}
