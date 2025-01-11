using AllkuApp.Services;
using AllkuApp.Vista;
using AllkuApp.Dtos;
using AllkuApp.Servicios;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ModeloReceta = AllkuApp.Modelo.Receta;
using ServicioReceta = AllkuApp.Services.Receta;
using CreateRecetaRequest = AllkuApp.Services.CreateRecetaRequest;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;
        private List<ModeloReceta> _recetas;
        private readonly RecetaService _recetaService;

        public MainPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _recetaService = new RecetaService();
            _recetas = new List<ModeloReceta>();

            // Suscribirse a las actualizaciones del perfil
            MessagingCenter.Subscribe<PerfilPage, object>(this, "PerfilActualizado", (sender, actualizacion) =>
            {
                var edad = (int)actualizacion.GetType().GetProperty("EdadCanino").GetValue(actualizacion);
                var peso = (decimal)actualizacion.GetType().GetProperty("PesoCanino").GetValue(actualizacion);
                var foto = (byte[])actualizacion.GetType().GetProperty("FotoCanino").GetValue(actualizacion);

                // Actualizar solo los campos modificables
                EdadCanino.Text = $"Edad: {edad} años";
                PesoCanino.Text = $"Peso: {peso} kg";

                if (foto != null && foto.Length > 0)
                {
                    FotoCanino.Source = ImageSource.FromStream(() => new MemoryStream(foto));
                }
            });


            CargarDatosCanino();
            CheckForNotifications();
            MessagingCenter.Subscribe<RecetaFormulario, ModeloReceta>(this, "RecetaCreada", (sender, receta) =>
            {
                _recetas.Add(receta);
                Console.WriteLine($"Receta agregada: {receta.nombre_receta}");
            });

            // Cargar las recetas al inicializar la página
            CargarRecetas();

        }

        private async void CargarRecetas()
        {
            try
            {
                var recetasServicio = await _recetaService.GetRecetasAsync();
                _recetas = ConvertirRecetas(recetasServicio);
                Console.WriteLine($"Número de recetas cargadas: {_recetas.Count}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un problema al cargar las recetas: {ex.Message}", "OK");
            }
        }

        private List<ModeloReceta> ConvertirRecetas(List<ServicioReceta> recetasServicio)
        {
            var recetasModelo = new List<ModeloReceta>();
            foreach (var receta in recetasServicio)
            {
                recetasModelo.Add(new ModeloReceta
                {
                    nombre_receta = receta.nombre_receta,
                    descripcion_receta = receta.descripcion_receta,
                    foto_receta = receta.foto_receta,
                    id_canino = receta.id_canino
                });
            }

            return recetasModelo;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarDatosCanino(); // Asegurarse de cargar datos al aparecer la página
            CheckForNotifications();
        }

        private async void CargarDatosCanino()
        {
            try
            {
                var cedulaDueno = Preferences.Get("CedulaDueno", string.Empty);
                if (string.IsNullOrEmpty(cedulaDueno))
                {
                    await DisplayAlert("Error", "No se pudo obtener la cédula del dueño.", "OK");
                    return;
                }

                var caninos = await _apiService.GetCaninosByCedulaDuenoAsync(cedulaDueno);
                if (caninos != null && caninos.Count > 0)
                {
                    var primerCanino = caninos[0];

                    // Asignar datos base (no modificables)
                    NombreCanino.Text = primerCanino.NombreCanino;
                    RazaCanino.Text = $"Raza: {primerCanino.RazaCanino}";

                    // Asignar datos actualizables (verificar si hay valores en Preferences)
                    var edadGuardada = Preferences.Get("EdadCanino", -1);
                    var pesoGuardado = Preferences.Get("PesoCanino", -1.0);
                    var fotoGuardada = Preferences.Get("FotoCanino", string.Empty);

                    // Usar valores de Preferences si existen, sino usar los de la base de datos
                    EdadCanino.Text = $"Edad: {(edadGuardada != -1 ? edadGuardada : primerCanino.EdadCanino)} años";
                    PesoCanino.Text = $"Peso: {(pesoGuardado != -1.0 ? pesoGuardado : primerCanino.PesoCanino)} kg";

                    if (!string.IsNullOrEmpty(fotoGuardada))
                    {
                        var fotoBytes = Convert.FromBase64String(fotoGuardada);
                        FotoCanino.Source = ImageSource.FromStream(() => new MemoryStream(fotoBytes));
                    }
                    else if (primerCanino.FotoCanino != null && primerCanino.FotoCanino.Length > 0)
                    {
                        FotoCanino.Source = ImageSource.FromStream(() => new MemoryStream(primerCanino.FotoCanino));
                    }

                    Preferences.Set("CaninoId", primerCanino.IdCanino);
                }
                else
                {
                    Debug.WriteLine("No se encontraron datos del canino.");
                    await DisplayAlert("Error", "No se encontraron datos del canino.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocurrió un problema: {ex.Message}");
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
            await Navigation.PushAsync(new RegistroHistorialPage());
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
            Console.WriteLine($"Número de recetas: {_recetas.Count}");

            if (_recetas.Count > 0)
            {
                var recetasCreateRequest = ConvertirRecetasARequest(_recetas);
                await Navigation.PushAsync(new RecetaPage(recetasCreateRequest));
            }
            else
            {
                await DisplayAlert("Error", "No hay recetas registradas.", "OK");
            }
        }

        private List<CreateRecetaRequest> ConvertirRecetasARequest(List<ModeloReceta> recetas)
        {
            var recetasRequest = new List<CreateRecetaRequest>();
            foreach (var receta in recetas)
            {
                recetasRequest.Add(new CreateRecetaRequest
                {
                    nombre_receta = receta.nombre_receta,
                    descripcion_receta = receta.descripcion_receta,
                    foto_receta = receta.foto_receta,
                    id_canino = receta.id_canino
                });
            }

            return recetasRequest;
        }
        

        private async void OnPerfilButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PerfilPage());
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