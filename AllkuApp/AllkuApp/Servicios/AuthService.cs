using Newtonsoft.Json;
using AllkuApp.Dtos;
using System;
using System.Net.Http;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace AllkuApp.Servicios
{
    public class AuthService
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "https://allkuapi.sytes.net/api/auth/";
        
        public AuthService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };

            // Configurar headers
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Configurar el tiempo de espera
            _client.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<bool> RegistrarAdministradorAsync(RegistrarAdministradorDto dto)
        {
            try
            {
                // Encriptar contraseña
                dto.ContrasenaAdministrador = HashingHelper.HashPassword(dto.ContrasenaAdministrador);
                Debug.WriteLine("Iniciando registro de administrador...");
                Debug.WriteLine($"Datos: {JsonConvert.SerializeObject(dto)}");

                var content = new StringContent(
                    JsonConvert.SerializeObject(dto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PostAsync("registrar-administrador", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"Código de respuesta: {response.StatusCode}");
                Debug.WriteLine($"Contenido de respuesta: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Administrador registrado correctamente.");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Error al registrar administrador. Status: {response.StatusCode}, Mensaje: {responseContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error de red al registrar administrador: {ex.Message}");
                throw new Exception("Error de conexión con el servidor. Verifica tu conexión a internet.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado al registrar administrador: {ex.Message}");
                throw new Exception("Error inesperado al registrar administrador.", ex);
            }
        }

        public async Task<bool> RegistrarUsuarioAsync(RegistrarUsuarioDto registerDto)
        {
            try
            {
                //Encriptar contraseña
                registerDto.Contrasena = HashingHelper.HashPassword(registerDto.Contrasena);
                Debug.WriteLine("Iniciando registro de usuario...");
                Debug.WriteLine($"Datos de registro: {JsonConvert.SerializeObject(registerDto)}");

                var content = new StringContent(
                    JsonConvert.SerializeObject(registerDto),
                    Encoding.UTF8,
                    "application/json"
                );

                // Asegurarse de que la URL coincida exactamente con el endpoint del backend
                var response = await _client.PostAsync("registrar-usuario", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"Código de respuesta: {response.StatusCode}");
                Debug.WriteLine($"Contenido de respuesta: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Usuario registrado correctamente.");
                    return true;
                }

                Debug.WriteLine($"Error al registrar usuario. Status: {response.StatusCode}, Mensaje: {responseContent}");
                return false;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error de red al registrar usuario: {ex.Message}");
                throw new Exception("Error de conexión con el servidor. Verifica tu conexión a internet.", ex);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error al procesar JSON: {ex.Message}");
                throw new Exception("Error al procesar los datos del usuario.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado al registrar usuario: {ex.Message}");
                throw new Exception($"Error inesperado al registrar usuario: {ex.Message}", ex);
            }
        }
    }
}