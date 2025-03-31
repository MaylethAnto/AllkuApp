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
using JsonException = System.Text.Json.JsonException;

namespace AllkuApp.Servicios
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl = "https://allkuapi-production.up.railway.app/api";

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
                var response = await _client.GetAsync($"{_baseUrl}/Canino");
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

        // Método para obtener los datos del dueño por cédula
        public async Task<Dueno> ObtenerDuenoPorCedulaAsync(string cedula)
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/Canino/usuarios?cedulaDueno={cedula}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var dueno = JsonConvert.DeserializeObject<Dueno>(content);
                    return dueno;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        // Método para obtener los caninos por cédula del dueño
        public async Task<List<CaninoDto>> GetCaninosByCedulaDuenoAsync(string cedulaDueno)
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/Canino/caninosPorCedula?cedulaDueno={cedulaDueno}");
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
                string url = $"{_baseUrl}/Canino/RegistrarCanino";
                var json = JsonConvert.SerializeObject(caninoRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

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

        // Métodos para notificaciones 
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
                var response = await _client.PostAsync($"{_baseUrl}/Notificacion/enviar", content);
                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Error al enviar la notificación: {responseContent}");
                }
                return (true, "Notificación enviada exitosamente.");
            }
            catch (HttpRequestException httpEx)
            {
                return (false, $"Error de solicitud HTTP: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<bool> CheckForNotificationsAsync()
        {
            var cedulaDueno = Preferences.Get("CedulaDueno", string.Empty);
            var response = await _client.GetAsync($"{_baseUrl}/Notificacion/check?cedulaDueno={cedulaDueno}");
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
            var response = await _client.GetAsync($"{_baseUrl}/Notificacion/ultima?cedulaDueno={cedulaDueno}");
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

            var response = await _client.PutAsync($"{_baseUrl}/Notificacion/marcarComoLeida", content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error al marcar la notificación como leída: {response.ReasonPhrase}");
            }
        }

        // Métodos para Ejercicio
        public async Task<DistanciaRecorridaModel> ObtenerDistanciaRecorridaAsync(int idCanino)
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/Gps/distancia/{idCanino}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<DistanciaRecorridaModel>(json);
                }
                else
                {
                    Debug.WriteLine($"Error: {response.ReasonPhrase}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public async Task<List<PaseoModel>> ObtenerPaseosFinalizadosAsync(int idCanino)
        {
            var url = $"{_baseUrl}/Gps/paseos-finalizados/{idCanino}";
            try
            {
                var response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Error: {response.ReasonPhrase}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"JSON recibido: {json}"); // Para verificar qué estás recibiendo

                // Intenta deserializar como lista primero
                try
                {
                    var result = JsonConvert.DeserializeObject<List<PaseoModel>>(json);
                    return result ?? new List<PaseoModel>();
                }
                catch
                {
                    // Si falla, intenta deserializar como un solo objeto
                    try
                    {
                        var singleResult = JsonConvert.DeserializeObject<PaseoModel>(json);
                        return singleResult != null ? new List<PaseoModel> { singleResult } : new List<PaseoModel>();
                    }
                    catch
                    {
                        // Si también falla, verifica si es un mensaje de error
                        try
                        {
                            var message = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                            if (message != null && message.ContainsKey("Message"))
                            {
                                Debug.WriteLine(message["Message"]);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error al intentar parsear el JSON: {ex.Message}");
                        }

                        return new List<PaseoModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PaseadorDisponibleDto>> GetPaseadoresDisponiblesAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/Paseador/disponibles");
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
                var response = await _client.PostAsync($"{_baseUrl}/Paseador/solicitud", content);
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
                var url = $"{_baseUrl}/Paseador/{cedula}/solicitudes";
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
                var response = await _client.PutAsync($"{_baseUrl}/Paseador/solicitud/{idSolicitud}/responder", content);
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
                var url = $"{_baseUrl}/Paseador/ObtenerIdPaseoPorIdSolicitud/{idSolicitud}";
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

                var url = $"{_baseUrl}/Paseador/paseo/{idPaseo}/finalizar";
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
                    $"{_baseUrl}/Paseador/paseo/{actualizacion.IdSolicitud}/{endpoint}",
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
                var response = await _client.GetAsync($"{_baseUrl}/perfiles");
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
                var response = await _client.PostAsync($"{_baseUrl}/perfiles", content);
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

        // Métodos para GPS
        public async Task<Gps> RegistrarGpsAsync(Gps gps)
        {
            try
            {
                // Validación exhaustiva del objeto GPS
                if (gps == null)
                    throw new ArgumentNullException(nameof(gps), "El objeto GPS no puede ser nulo");

                if (gps.id_canino <= 0)
                    throw new ArgumentException($"ID de canino no válido: {gps.id_canino}");

                if (gps.iniciolatitud == 0 || gps.iniciolongitud == 0)
                    throw new ArgumentException("Las coordenadas iniciales no pueden ser cero");

                // Configurar serialización
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ" // Formato ISO 8601
                };

                var json = JsonConvert.SerializeObject(gps, jsonSettings);
                Debug.WriteLine($"JSON a enviar: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}/Gps", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Respuesta del servidor: {response.StatusCode} - {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = $"Error al registrar GPS: {response.StatusCode}";
                    try
                    {
                        var errorObj = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        errorMessage += $". Detalles: {errorObj?.message ?? errorObj?.Message ?? responseContent}";
                    }
                    catch
                    {
                        errorMessage += $". Respuesta: {responseContent}";
                    }

                    throw new HttpRequestException(errorMessage);
                }

                return JsonConvert.DeserializeObject<Gps>(responseContent);
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"Error HTTP: {httpEx.Message}");
                throw new Exception("Problema de comunicación con el servidor. Verifica tu conexión.");
            }
            catch (JsonException jsonEx)
            {
                Debug.WriteLine($"Error JSON: {jsonEx.Message}");
                throw new Exception("Error procesando la respuesta del servidor.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado: {ex}");
                throw new Exception("Ocurrió un error inesperado al registrar el GPS.");
            }
        }
        public async Task ActualizarGpsAsync(Gps gps)
        {
            try
            {
                // Verificar que el ID sea válido
                Debug.WriteLine($"Actualizando GPS con ID: {gps.id_gps}");
                if (gps.id_gps <= 0)
                {
                    throw new ArgumentException($"ID de GPS no válido: {gps.id_gps}");
                }

                // Verificar que las coordenadas no sean cero o valores extremos
                Debug.WriteLine($"Coordenadas finales: Lat: {gps.finlatitud}, Long: {gps.finlongitud}");

                var datosFinales = new
                {
                    FinLatitud = gps.finlatitud,
                    FinLongitud = gps.finlongitud
                };

                // Usar System.Text.Json en lugar de Newtonsoft si es posible
                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                var json = System.Text.Json.JsonSerializer.Serialize(datosFinales, jsonOptions);
                Debug.WriteLine($"Enviando datos: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var url = $"{_baseUrl}/Gps/finalizar/{gps.id_gps}";
                Debug.WriteLine($"URL: {url}");

                // Establecer un timeout más largo
                _client.Timeout = TimeSpan.FromSeconds(30);

                var response = await _client.PutAsync(url, content);
                Debug.WriteLine($"Código de respuesta: {(int)response.StatusCode} {response.StatusCode}");

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Contenido de respuesta: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Error del servidor: {(int)response.StatusCode} {response.StatusCode} - {responseContent}");
                    throw new HttpRequestException($"Error al actualizar GPS: {response.StatusCode}. Respuesta: {responseContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error de HTTP: {ex.Message}");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"Timeout al comunicarse con el servidor: {ex.Message}");
                throw new Exception("Tiempo de espera agotado al comunicarse con el servidor. Verifica tu conexión de Internet.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado: {ex.GetType().Name} - {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
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
