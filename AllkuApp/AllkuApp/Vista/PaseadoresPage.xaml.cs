using AllkuApp.Dtos;
using AllkuApp.Servicios;
using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Diagnostics;
using System.IO;

namespace AllkuApp.Vista
{
    public partial class PaseadoresPage : ContentPage
    {
        private readonly ApiService _apiService;
        private CaninoDto _canSeleccionado;

        public PaseadoresPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            CargarCanes();
        }

        private async void CargarCanes()
        {
            var canes = await _apiService.GetCaninesAsync();
            foreach (var canino in canes)
            {
                // Convertir byte array a ImageSource
                if (canino.FotoCanino != null)
                {
                    canino.FotoCaninoImageSource = ImageSource.FromStream(() => new MemoryStream(canino.FotoCanino));
                }
            }
            CanesListView.ItemsSource = canes;
        }

        private void OnCanSelected(object sender, SelectedItemChangedEventArgs e)
        {
            _canSeleccionado = e.SelectedItem as CaninoDto;
        }

        private async void OnEnviarNotificacionClicked(object sender, EventArgs e)
        {
            try
            {
                if (_canSeleccionado == null)
                {
                    await DisplayAlert("Error", "Seleccione un can para pasear.", "OK");
                    return;
                }

                Debug.WriteLine("=== Intentando recuperar información del paseador ===");

                var celularPaseador = Preferences.Get("CelularPaseador", null);
                var nombrePaseador = Preferences.Get("NombrePaseador", null);
                Debug.WriteLine($"Celular recuperado: {celularPaseador}");
                Debug.WriteLine($"Nombre recuperado: {nombrePaseador}");

                if (string.IsNullOrEmpty(celularPaseador) || string.IsNullOrEmpty(nombrePaseador))
                {
                    Debug.WriteLine("ERROR: No se encontró la información del paseador en las preferencias");
                    await DisplayAlert("Error", "No se pudo recuperar la información del paseador.", "OK");
                    return;
                }

                Debug.WriteLine($"Información del paseador recuperada exitosamente: {nombrePaseador}, {celularPaseador}");

                var mensaje = $"Hola, me llamo {nombrePaseador} quiero pasear a tu can {_canSeleccionado.NombreCanino}.\nContactame al siguiente número por favor: ";

                // Enviar la notificación a través de la API
                var (exito, message) = await _apiService.EnviarNotificacionAsync(_canSeleccionado.IdCanino, mensaje, celularPaseador);

                if (exito)
                {
                    await DisplayAlert("Éxito", message, "OK");
                }
                else
                {
                    await DisplayAlert("Error", message, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al procesar la notificación: {ex.Message}");
                await DisplayAlert("Error", "Ocurrió un error al procesar la solicitud.", "OK");
            }
        }

        private async void OnMenuPaseador_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MenuPaseadorPage());
        }
    }
}