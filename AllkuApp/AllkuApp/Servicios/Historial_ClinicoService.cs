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
        private readonly string _baseUrl = "https://allkuapi.sytes.net/api/HistorialClinico";

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
        public async Task<bool> CreateHistorialClinicoAsync(HistorialRequest historialRequest)
        {
            try
            {
                // Log de los datos que se van a enviar
                Debug.WriteLine($"Enviando datos del historial:");
                Debug.WriteLine($"Fecha: {historialRequest.fecha_historial}");
                Debug.WriteLine($"Tipo: {historialRequest.tipo_historial}");
                Debug.WriteLine($"Descripción: {historialRequest.descripcion_historial}");
                Debug.WriteLine($"Notificación: {historialRequest.notificacion_historial}");
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

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción al crear historial: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
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
        public bool notificacion_historial { get; set; }
        public int id_canino { get; set; }
    }

    public class HistorialRequest
    {
        public DateTime fecha_historial { get; set; }
        public string tipo_historial { get; set; }
        public string descripcion_historial { get; set; }
        public bool notificacion_historial { get; set; }
        public int id_canino { get; set; }
    }
}