using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AllkuApp.Servicios
{
    public class Historial_ClinicoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://10.0.2.2:5138/api/HistorialClinico";
        private readonly string _notificacionesUrl = "http://10.0.2.2:5138/api/Notificaciones";

        public Historial_ClinicoService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Obtener todos los historiales clínicos
        public async Task<List<HistorialClinicoDto>> GetHistorialesClinicosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var historiales = JsonConvert.DeserializeObject<List<HistorialClinicoDto>>(content);
                    return historiales;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener historiales clínicos: {ex.Message}");
            }
        }

        // Obtener historiales clínicos por ID de canino
        public async Task<List<HistorialClinicoDto>> GetHistorialesPorCaninoAsync(int idCanino)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Canino/{idCanino}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var historiales = JsonConvert.DeserializeObject<List<HistorialClinicoDto>>(content);
                    return historiales;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener historiales del canino: {ex.Message}");
            }
        }

        // Crear nuevo historial clínico
        public async Task<bool> CreateHistorialClinicoAsync(HistorialRequest historialRequest, bool enviarNotificacion)
        {
            try
            {
                // Log de los datos que se van a enviar
                Debug.WriteLine($"Enviando datos del historial:");
                Debug.WriteLine($"Fecha: {historialRequest.fecha_historial}");
                Debug.WriteLine($"Tipo: {historialRequest.tipo_historial}");
                Debug.WriteLine($"Descripción: {historialRequest.descripcion_historial}");
                Debug.WriteLine($"ID Canino: {historialRequest.id_canino}");

                var json = JsonConvert.SerializeObject(historialRequest);
                Debug.WriteLine($"JSON a enviar: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Log de la URL
                Debug.WriteLine($"URL de la petición: {_baseUrl}");

                var response = await _httpClient.PostAsync(_baseUrl, content);

                // Log de la respuesta
                Debug.WriteLine($"Código de estado: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Contenido de la respuesta: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Error en la respuesta: {responseContent}");
                    Debug.WriteLine($"Status Code: {response.StatusCode}");
                    return false;
                }

                if (enviarNotificacion)
                {
                    // Enviar notificación
                    await EnviarNotificacionAsync(historialRequest);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción al crear historial: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task EnviarNotificacionAsync(HistorialRequest historialRequest)
        {
            var notification = new
            {
                mensaje = $"Recordatorio: {historialRequest.descripcion_historial}",
                cedulaDueno = historialRequest.id_canino, // Ajusta esto según tu lógica de negocio
                fecha = historialRequest.fecha_historial.ToString("yyyy-MM-ddTHH:mm:ss"),
                leida = false
            };

            var json = JsonConvert.SerializeObject(notification);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_notificacionesUrl, content);
            response.EnsureSuccessStatusCode();
        }

        // Marcar notificación como leída
        public async Task<bool> MarcarNotificacionComoLeidaAsync(int idNotificacion)
        {
            try
            {
                var response = await _httpClient.PutAsync($"{_notificacionesUrl}/{idNotificacion}/leida", null);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al marcar la notificación como leída: {ex.Message}");
            }
        }

        // Obtener notificaciones
        public async Task<List<NotificacionDto>> GetNotificacionesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_notificacionesUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var notificaciones = JsonConvert.DeserializeObject<List<NotificacionDto>>(content);
                    return notificaciones;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener notificaciones: {ex.Message}");
            }
        }

        // Actualizar historial clínico
        public async Task<bool> UpdateHistorialClinicoAsync(int id, HistorialRequest historialRequest)
        {
            try
            {
                Debug.WriteLine($"Actualizando historial con ID: {id}");
                Debug.WriteLine($"Nuevos datos del historial:");
                Debug.WriteLine($"Fecha: {historialRequest.fecha_historial}");
                Debug.WriteLine($"Tipo: {historialRequest.tipo_historial}");
                Debug.WriteLine($"Descripción: {historialRequest.descripcion_historial}");
                Debug.WriteLine($"ID Canino: {historialRequest.id_canino}");

                var json = JsonConvert.SerializeObject(historialRequest);
                Debug.WriteLine($"JSON a enviar: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/{id}", content);

                Debug.WriteLine($"Código de estado: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Contenido de la respuesta: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Error en la respuesta: {responseContent}");
                    Debug.WriteLine($"Status Code: {response.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción al actualizar historial: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw new Exception($"Error al actualizar historial clínico: {ex.Message}");
            }
        }

        // Eliminar historial clínico
        public async Task<bool> DeleteHistorialClinicoAsync(int id)
        {
            try
            {
                Debug.WriteLine($"Eliminando historial con ID: {id}");

                var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");

                Debug.WriteLine($"Código de estado: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Contenido de la respuesta: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Error en la respuesta: {responseContent}");
                    Debug.WriteLine($"Status Code: {response.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción al eliminar historial: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw new Exception($"Error al eliminar historial clínico: {ex.Message}");
            }
        }
    }

    // Clases DTO necesarias
    public class HistorialClinicoDto
    {
        public int id_historial { get; set; }
        public DateTime fecha_historial { get; set; }
        public string tipo_historial { get; set; }
        public string descripcion_historial { get; set; }
        public int id_canino { get; set; }
    }

    public class HistorialRequest
    {
        public DateTime fecha_historial { get; set; }
        public string tipo_historial { get; set; }
        public string descripcion_historial { get; set; }
        public int id_canino { get; set; }
    }

    public class NotificacionDto
    {
        public int IdNotificacion { get; set; }
        public string Mensaje { get; set; }
        public string CedulaDueno { get; set; }
        public DateTime Fecha { get; set; }
        public bool Leida { get; set; }
    }
}