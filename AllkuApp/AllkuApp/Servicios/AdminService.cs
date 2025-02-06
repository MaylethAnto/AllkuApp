using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Xamarin.Forms;
using AllkuApp.Modelo;


namespace AllkuApp.Servicios
{
    public class AdminService
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl = "http://10.0.2.2:5138/api/Administrador";
        private readonly ContentPage _page;

        public AdminService(ContentPage page)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _page = page;
        }

        public async Task<List<T>> ObtenerDatos<T>(string tipoUsuario)
        {
            try
            {
                string endpoint = tipoUsuario.ToLower();
                string url = $"{_baseUrl}/{endpoint}";
                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Aquí verificamos el tipo y deserializamos correctamente
                    if (typeof(T) == typeof(Paseador))
                    {
                        return JsonConvert.DeserializeObject<List<Paseador>>(content) as List<T>;
                    }
                    else if (typeof(T) == typeof(Canino))
                    {
                        return JsonConvert.DeserializeObject<List<Canino>>(content) as List<T>;
                    }
                    else if (typeof(T) == typeof(Dueno))
                    {
                        return JsonConvert.DeserializeObject<List<Dueno>>(content) as List<T>;
                    }
                    else
                    {
                        return new List<T>(); // Retornar vacío si no se puede deserializar
                    }
                }
                else
                {
                    await _page.DisplayAlert("Error", $"Error al obtener {tipoUsuario}: {response.StatusCode} - {response.ReasonPhrase}", "Aceptar");
                    return new List<T>();
                }
            }
            catch (Exception ex)
            {
                await _page.DisplayAlert("Error", $"Error al obtener {tipoUsuario}: {ex.Message}", "Aceptar");
                return new List<T>();
            }
        }

        public async Task<bool> TogglePaseadorEstado(string cedula)
        {
            try
            {
                string url = $"{_baseUrl}/paseadores/{cedula}/estado";
                var response = await _client.PutAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return true;
                }
                else
                {
                    await _page.DisplayAlert("Error", $"No se pudo cambiar el estado: {response.StatusCode}", "Aceptar");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await _page.DisplayAlert("Error", $"Error: {ex.Message}", "Aceptar");
                return false;
            }
        }

    }
}