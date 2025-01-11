using AllkuApp.Modelo;
using Newtonsoft.Json;
using AllkuApp.Modelo;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AllkuApp.Servicios
{
    public class CaninoHistorialService
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "https://allkuapi.sytes.net/api/Canino";

        // Constructor para inicializar HttpClient
        public CaninoHistorialService()
        {
            _client = new HttpClient();
        }

        // Método para obtener caninos con historial clínico
        public async Task<List<CaninoConHistorialDTO>> ObtenerCaninosConHistorialAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{BaseUrl}/CaninosConHistorial");
                response.EnsureSuccessStatusCode(); // Lanza una excepción si el código de estado no es exitoso
                var jsonResponse = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Response JSON: {jsonResponse}");
                return JsonConvert.DeserializeObject<List<CaninoConHistorialDTO>>(jsonResponse);
            }
            catch (HttpRequestException e)
            {
                // Manejo de errores HTTP
                throw new Exception("Error al obtener los caninos con historial clínico.", e);
            }
        }

        // Método para crear un historial clínico
        public async Task<bool> CrearHistorialClinicoAsync(HistorialRequest historialRequest)
        {
            try
            {
                var json = JsonConvert.SerializeObject(historialRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{BaseUrl}/RegistrarHistorialClinico", content);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException e)
            {
                // Manejo de errores HTTP
                throw new Exception("Error al crear el historial clínico.", e);
            }
        }
    }


    public class CaninoConHistorialDTO
    {
        public int idCanino { get; set; }
        public string nombreCanino { get; set; }
        public string razaCanino { get; set; }
        public int edadCanino { get; set; }
        public double pesoCanino { get; set; }
        public int id_historial { get; set; }
        public DateTime fecha_historial { get; set; }
        public string tipo_historial { get; set; }
        public string descripcion_historial { get; set; }
        public bool notificacion_historial { get; set; }

    }
}