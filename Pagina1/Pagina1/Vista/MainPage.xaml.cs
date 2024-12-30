using Pagina1.Servicios;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pagina1.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;

        public MainPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            CargarDatosCanino();
            CheckForNotifications();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CheckForNotifications();
        }
        private async void CargarDatosCanino()
        {
            try
            {
                // Leer la cédula del dueño desde Preferences
                var cedulaDueno = Preferences.Get("CedulaDueno", string.Empty);
                if (string.IsNullOrEmpty(cedulaDueno))
                {
                    await DisplayAlert("Error", "No se pudo obtener la cédula del dueño.", "OK");
                    return;
                }

                // Consumimos el API para obtener los datos del canino
                var caninos = await _apiService.GetCaninosByCedulaDuenoAsync(cedulaDueno);
                if (caninos != null && caninos.Count > 0)
                {
                    // Asignamos los datos a los controles (aquí se asume que se toma el primer canino de la lista)
                    var primerCanino = caninos[0];
                    NombreCanino.Text = primerCanino.NombreCanino;
                    EdadCanino.Text = $"Edad: {primerCanino.EdadCanino} años";
                    RazaCanino.Text = $"Raza: {primerCanino.RazaCanino}";
                    PesoCanino.Text = $"Peso: {primerCanino.PesoCanino} kg";
                    if (primerCanino.FotoCanino != null && primerCanino.FotoCanino.Length > 0)
                    {
                        FotoCanino.Source = ImageSource.FromStream(() => new MemoryStream(primerCanino.FotoCanino));
                    }
                    else
                    {
                        Debug.WriteLine("La foto del canino está vacía o es nula.");
                    }
                }
                else
                {
                    Debug.WriteLine("No se encontraron datos del canino."); // Mensaje de depuración
                    await DisplayAlert("Error", "No se encontraron datos del canino.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocurrió un problema: {ex.Message}"); // Mensaje de depuración
                await DisplayAlert("Error", $"Ocurrió un problema: {ex.Message}", "OK");
            }
        }

        private async void CheckForNotifications()
        {
            var tieneNotificacion = await _apiService.CheckForNotificationsAsync();
            NotificationDot.IsVisible = tieneNotificacion;
        }

        private async void OnBellIconTapped(object sender, EventArgs e)
        {
            var notificacion = await _apiService.GetLatestNotificationAsync();
            if (notificacion != null)
            {
                await Navigation.PushAsync(new NotificacionPage(notificacion.Mensaje, notificacion.NumeroPaseador, notificacion.IdNotificacion));
            }
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            // Navegar a la página de menú
            await Navigation.PushAsync(new MenuPage());
        }

        protected override bool OnBackButtonPressed()
        {
            // Cerrar sesión si se presiona el botón de retroceso
            SecureStorage.Remove("user_token");
            SecureStorage.Remove("user_id");
            Application.Current.MainPage = new NavigationPage(new LoginPage());
            return true; // Indicar que el evento del botón de retroceso ha sido manejado
        }

        private async void OnRegistrarMascotaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistroMascotaPage());
        }

        private async void OnHistorialClinicoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HistorialClinicoPage());
        }

        private async void OnGPSClicked(object sender, EventArgs e)
        {
            // Navegamos a la interfaz del GPS
            await Navigation.PushAsync(new GPSPage());
        }

        private async void OnEjerciciosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EjerciciosPage());
        }

        private async void OnPaseadoresClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PaseadoresPage());
        }

        private async void OnRecetasClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RecetasPage());
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool logout = await DisplayAlert("Cerrar Sesión", "¿Está seguro que desea cerrar sesión?", "Sí", "No");
            if (logout)
            {
                await Navigation.PopToRootAsync();
            }
        }
    }
}