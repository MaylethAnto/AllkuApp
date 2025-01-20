using Xamarin.Essentials;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AllkuApp.Modelo;

namespace AllkuApp.Servicios
{
    public class LoginResponse
    {
        public string Mensaje { get; set; }
        public string Rol { get; set; }
        public string CedulaDueno { get; set; }
        public string CedulaPaseador { get; set; }
        public string CelularPaseador { get; set; }
        public string NombrePaseador { get; set; }
        public bool EsPrimeraVez { get; set; }
        public DateTime? UltimoInicioSesion { get; set; }
        public PerfilCanino PerfilCanino { get; set; } // Agrega esta propiedad para el perfil del canino
    }

    public class LoginService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://allkuapi.sytes.net/api/Login/Login";
        private const string PerfilCaninoKey = "PerfilCanino";

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
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                    // Guardar los datos del perfil del canino en las preferencias
                    if (loginResponse.PerfilCanino != null)
                    {
                        var perfilJson = JsonConvert.SerializeObject(loginResponse.PerfilCanino);
                        Preferences.Set(PerfilCaninoKey, perfilJson);
                    }

                    return loginResponse;
                }
                else
                {
                    return new LoginResponse
                    {
                        Mensaje = "Error al iniciar sesión",
                        Rol = "",
                        CedulaDueno = string.Empty,
                        CedulaPaseador = string.Empty,
                        CelularPaseador = string.Empty,
                        NombrePaseador = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Mensaje = $"Error de conexión: {ex.Message}",
                    Rol = "",
                    CedulaDueno = string.Empty,
                    CedulaPaseador = string.Empty,
                    CelularPaseador = string.Empty,
                    NombrePaseador = string.Empty
                };
            }
        }

        public async Task<bool> EsPrimeraVez(string nombreUsuario)
        {
            var response = await _httpClient.GetAsync($"https://allkuapi.sytes.net/api/Login/esprimavez?username={nombreUsuario}");

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