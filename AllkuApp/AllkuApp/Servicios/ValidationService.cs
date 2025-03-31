using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AllkuApp.Servicios
{
    public class ValidationService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://allkuapi-production.up.railway.app"; // Replace with your actual API base URL

        public ValidationService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> VerificarCedulaAsync(string cedula)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Validacion/verificar-cedula/{cedula}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeAnonymousType(content, new { existe = false });
                    return result.existe;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al verificar cédula: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> VerificarUsuarioAsync(string usuario)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Validacion/verificar-usuario/{usuario}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeAnonymousType(content, new { existe = false });
                    return result.existe;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al verificar usuario: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> VerificarCorreoAsync(string correo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Validacion/verificar-correo/{correo}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeAnonymousType(content, new { existe = false });
                    return result.existe;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al verificar correo: {ex.Message}");
                throw;
            }
        }
    }
}