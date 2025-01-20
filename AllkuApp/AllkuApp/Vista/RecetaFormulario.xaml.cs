using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;
using AllkuApp.Modelo;

namespace AllkuApp.Vista
{
    public partial class RecetaFormulario : ContentPage
    {
        private readonly HttpClient _httpClient;
        private byte[] _fotoRecetaBytes;
        private bool _isSaving;

        public RecetaFormulario()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://allkuapi.sytes.net/api/Recetas");
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navegar de regreso a la página anterior
            await Navigation.PopAsync();
        }

        private async void OnSelectPhotoClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();
                if (photo != null)
                {
                    var stream = await photo.OpenReadAsync();
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    _fotoRecetaBytes = memoryStream.ToArray();

                    // Mostrar la imagen seleccionada
                    FotoRecetaImage.Source = ImageSource.FromStream(() => new MemoryStream(_fotoRecetaBytes));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "No se pudo cargar la imagen", "OK");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_isSaving) return;
            _isSaving = true;

            try
            {
                if (string.IsNullOrWhiteSpace(NombreRecetaEntry.Text) ||
                    string.IsNullOrWhiteSpace(DescripcionRecetaEditor.Text) ||
                    string.IsNullOrWhiteSpace(IdCaninoEntry.Text))
                {
                    await DisplayAlert("Error", "Por favor complete todos los campos", "OK");
                    return;
                }

                var content = new MultipartFormDataContent();
                content.Add(new StringContent(NombreRecetaEntry.Text), "nombre_receta");
                content.Add(new StringContent(DescripcionRecetaEditor.Text), "descripcion_receta");
                content.Add(new StringContent(IdCaninoEntry.Text), "id_canino");

                if (_fotoRecetaBytes != null)
                {
                    var imageContent = new ByteArrayContent(_fotoRecetaBytes);
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(imageContent, "foto_receta", "foto.jpg");
                }

                var response = await _httpClient.PostAsync("Recetas", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Receta guardada correctamente", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo guardar la receta", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Ocurrió un error al guardar la receta", "OK");
            }
            finally
            {
                _isSaving = false;
            }
        }
    }
}