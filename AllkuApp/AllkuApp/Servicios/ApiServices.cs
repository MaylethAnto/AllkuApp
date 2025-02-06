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
        private readonly string _baseUrl = "http://10.0.2.2:5138/api/Canino";


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
                var response = await _client.GetAsync($"http://10.0.2.2:5138/api/Canino/usuarios?cedulaDueno={cedula}");

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
                var response = await _client.PostAsync("http://10.0.2.2:5138/api/Notificacion/enviar", content);

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
            var response = await _client.GetAsync($"http://10.0.2.2:5138/api/Notificacion/check?cedulaDueno={cedulaDueno}");
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
            var response = await _client.GetAsync($"http://10.0.2.2:5138/api/Notificacion/ultima?cedulaDueno={cedulaDueno}");
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

            var response = await _client.PutAsync($"http://10.0.2.2:5138/api/Notificacion/marcarComoLeida", content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error al marcar la notificación como leída: {response.ReasonPhrase}");
            }
        }

        // Métodos para Ejercicio
        public async Task<List<DistanciaRecorridaModel>> ObtenerDistanciasAsync(int idCanino)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"http://10.0.2.2:5138/api/Gps/distancia?id_canino={idCanino}");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<DistanciaRecorridaModel>>(json);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error al consumir el API: {ex.Message}");
                return null;
            }
        }

        public async Task<List<PaseoModel>> ObtenerPaseosFinalizadosAsync(int idCanino)
        {
            // Cambiamos la URL para usar el parámetro en la ruta en lugar de query parameter
            var url = $"http://10.0.2.2:5138/api/Gps/paseos-finalizados/{idCanino}";
            try
            {
                var response = await _client.GetStringAsync(url);
                Debug.WriteLine($"Respuesta API: {response}");

                var paseos = JsonConvert.DeserializeObject<List<PaseoModel>>(response);
                return paseos ?? new List<PaseoModel>();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error de HTTP: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado: {ex.Message}");
                throw;
            }
        }



        public async Task<List<PaseadorDisponibleDto>> GetPaseadoresDisponiblesAsync()
        {
            try
            {
                var response = await _client.GetAsync($"http://10.0.2.2:5138/api/Paseador/disponibles");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<PaseadorDisponibleDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                return new List<PaseadorDisponibleDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetPaseadoresDisponiblesAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> EnviarSolicitudPaseoAsync(SolicitudPaseoDto solicitud)
        {
            try
            {
                var json = JsonSerializer.Serialize(solicitud);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"http://10.0.2.2:5138/api/Paseador/solicitud", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en EnviarSolicitudPaseoAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<SolicitudPaseoResponseDto>> GetSolicitudesPaseadorAsync(string cedula)
        {
            try
            {
                var url = $"http://10.0.2.2:5138/api/Paseador/{cedula}/solicitudes";
                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var solicitudes = JsonConvert.DeserializeObject<List<SolicitudPaseoResponseDto>>(responseString);
                    return solicitudes;
                }
                else
                {
                    Debug.WriteLine($"Error al obtener solicitudes: {response.StatusCode}");
                    return new List<SolicitudPaseoResponseDto>(); // Si la respuesta no es exitosa, retornar lista vacía
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener solicitudes: {ex.Message}");
                return new List<SolicitudPaseoResponseDto>(); // Retorna lista vacía en caso de error
            }
        }




        public async Task<bool> ResponderSolicitudAsync(int idSolicitud, string cedulaPaseador, bool aceptada)
        {
            try
            {

                var respuesta = new RespuestaSolicitudDto
                {
                    CedulaPaseador = cedulaPaseador,
                    Aceptada = aceptada
                };
                var json = JsonSerializer.Serialize(respuesta);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"http://10.0.2.2:5138/api/Paseador/solicitud/{idSolicitud}/responder", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ResponderSolicitudAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<int?> ObtenerIdPaseoPorIdSolicitudAsync(int idSolicitud)
        {
            try
            {
                var url = $"http://10.0.2.2:5138/api/Paseador/ObtenerIdPaseoPorIdSolicitud/{idSolicitud}";
                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var idPaseo = JsonSerializer.Deserialize<int>(content);
                    return idPaseo;
                }

                Debug.WriteLine($"Error al obtener ID de paseo: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool success, string message)> FinalizarPaseoAsync(int idPaseo, string cedulaPaseador)
        {
            try
            {
                if (idPaseo <= 0 || string.IsNullOrEmpty(cedulaPaseador))
                {
                    throw new ArgumentException("ID de paseo debe ser un número entero positivo y cédula del paseador no puede estar vacía.");
                }

                var url = $"http://10.0.2.2:5138/api/paseador/paseo/{idPaseo}/finalizar";
                var requestBody = new { CedulaPaseador = cedulaPaseador };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _client.PutAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Paseo finalizado correctamente.");
                }
                else
                {
                    return (false, $"Error del servidor: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado: {ex.Message}");
                return (false, $"Error inesperado: {ex.Message}");
            }
        }




        public async Task<bool> ActualizarEstadoPaseoAsync(ActualizacionPaseoDto actualizacion)
        {
            try
            {
                var json = JsonSerializer.Serialize(actualizacion);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var endpoint = actualizacion.EsFinalizar ? "finalizar" : "iniciar";

                var response = await _client.PutAsync(
                    $"{_baseUrl}paseador/paseo/{actualizacion.IdSolicitud}/{endpoint}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ActualizarEstadoPaseoAsync: {ex.Message}");
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


    public class PaseadorDisponibleDto
    {
        public string CedulaPaseador { get; set; }
        public string NombrePaseador { get; set; }
        public string CelularPaseador { get; set; }
        public string CorreoPaseador { get; set; }
        public bool EstaDisponible { get; set; }
    }

   


    public class PaseoDto
    {
        public int IdPaseo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string EstadoPaseo { get; set; }
        public string NombreCanino { get; set; }
    }

    public class FinalizarPaseoDto
    {

        public string CedulaPaseador { get; set; }

    }


    public class RespuestaSolicitudDto
    {
        public int IdSolicitud { get; set; }
        public string CedulaPaseador { get; set; }
        public bool Aceptada { get; set; }
    }

    public class SolicitudPaseoDto
    {
        public int IdCanino { get; set; }
        public string CedulaPaseador { get; set; }
        public DateTime FechaSolicitud { get; set; }
    }

    public class ActualizacionPaseoDto
    {
        public int IdSolicitud { get; set; }
        public string CedulaPaseador { get; set; }
        public bool EsFinalizar { get; set; }
    }

}
