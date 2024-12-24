using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pagina1.Modelo;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Xamarin.Essentials;
using Pagina1.Dtos;

namespace Pagina1.Servicios
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
        public async Task<List<Canino>> GetCaninesAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}caninos");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Canino>>(json);
                }
                return null;
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
        public async Task<List<Ejercicio>> GetEjerciciosAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}ejercicios");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Ejercicio>>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Ejercicio> AddEjercicioAsync(Ejercicio ejercicio)
        {
            try
            {
                var json = JsonConvert.SerializeObject(ejercicio);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}ejercicios", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Ejercicio>(jsonResponse);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
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
}
