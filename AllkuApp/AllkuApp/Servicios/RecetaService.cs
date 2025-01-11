﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AllkuApp.Services
{
    public class RecetaService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://allkuapi.sytes.net/api/Recetas";

        public RecetaService()
        {
            _httpClient = new HttpClient();
        }

        // Método para obtener la lista de recetas
        public async Task<List<Receta>> GetRecetasAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al obtener las recetas");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Receta>>(jsonResponse);
        }

        // Método para obtener una receta por ID
        public async Task<Receta> GetRecetaByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al obtener la receta");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Receta>(jsonResponse);
        }

        // Método para crear una nueva receta
        public async Task<bool> CreateRecetaAsync(CreateRecetaRequest createRecetaRequest)
        {
            var jsonRequest = JsonConvert.SerializeObject(createRecetaRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(BaseUrl, content);

            return response.IsSuccessStatusCode;
        }

        // Método para actualizar una receta existente
        public async Task<bool> UpdateRecetaAsync(int id, RecetaRequest recetaRequest)
        {
            var jsonRequest = JsonConvert.SerializeObject(recetaRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{BaseUrl}/{id}", content);

            return response.IsSuccessStatusCode;
        }

        // Método para borrar una receta por ID
        public async Task<bool> DeleteRecetaAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

            return response.IsSuccessStatusCode;
        }
    }

    // Modelo de Receta
    public class Receta
    {
        public int id_receta { get; set; }
        public string nombre_receta { get; set; }
        public string descripcion_receta { get; set; }
        public byte[] foto_receta { get; set; }
        public int id_canino { get; set; }
    }

    public class CreateRecetaRequest
    {
        public string nombre_receta { get; set; }
        public string descripcion_receta { get; set; }
        public byte[] foto_receta { get; set; }
        public int id_canino { get; set; }
    }


    // Modelo de solicitud para actualizar una receta
    public class RecetaRequest
    {
        public int id_receta { get; set; }
        public string nombre_receta { get; set; }
        public string descripcion_receta { get; set; }
        public byte[] foto_receta { get; set; }
        public int id_canino { get; set; }
    }
}