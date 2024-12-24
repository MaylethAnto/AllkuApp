using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pagina1.Servicios
{
    public class LoginResponse
    {
        public string Mensaje { get; set; }
        public string Rol { get; set; }
        public string CedulaDueno { get; set; }
        public bool EsPrimeraVez { get; set; }
        public DateTime? UltimoInicioSesion { get; set; }
    }

    public class LoginService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "http://10.0.2.2:5138/api/Login/Login";

        public LoginService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<LoginResponse> LoginUsuarioAsync(string nombreUsuario, string contrasena)
        {
            // Encriptar contraseña usando HashingHelper
            contrasena = HashingHelper.HashPassword(contrasena);

            var loginRequest = new LoginRequest
            {
                NombreUsuario = nombreUsuario,
                Contrasena = contrasena
            };

            try
            {
                var json = JsonConvert.SerializeObject(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Request JSON: {json}");

                var response = await _httpClient.PostAsync(ApiUrl, content);

                Console.WriteLine($"Response Status: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                }
                else
                {
                    return new LoginResponse
                    {
                        Mensaje = "Error al iniciar sesión",
                        Rol = "",
                        CedulaDueno = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Mensaje = $"Error de conexión: {ex.Message}",
                    Rol = "",
                    CedulaDueno = string.Empty
                };
            }
        }

        public async Task<bool> EsPrimeraVez(string nombreUsuario)
        {
            var response = await _httpClient.GetAsync($"http://10.0.2.2:5138/api/Login/esprimavez?username={nombreUsuario}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var primeraVezResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                return primeraVezResponse.EsPrimeraVez;
            }
            else
            {
                throw new Exception("Error al verificar si es la primera vez que el usuario inicia sesión");
            }
        }
    }

    public class LoginRequest
    {
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
    }
}