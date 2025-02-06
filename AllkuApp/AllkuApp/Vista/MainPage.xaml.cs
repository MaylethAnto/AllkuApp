using AllkuApp.Services;
using AllkuApp.Vista;
using AllkuApp.Dtos;
using AllkuApp.Modelo;
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
        private List<Canino> caninos;
        private int currentIndex = 0;


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
            BindingContext = new LoginPageViewModel();

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

        private List<Canino> ConvertirCaninos(List<CaninoDto> caninosDto)
        {
            var caninosModelo = new List<Canino>();
            foreach (var caninoDto in caninosDto)
            {
                caninosModelo.Add(new Canino
                {
                    IdCanino = caninoDto.IdCanino,
                    NombreCanino = caninoDto.NombreCanino,
                    RazaCanino = caninoDto.RazaCanino,
                    EdadCanino = caninoDto.EdadCanino,
                    PesoCanino = (decimal)caninoDto.PesoCanino, // Conversión explícita a decimal
                    FotoCanino = caninoDto.FotoCanino
                });
            }
            return caninosModelo;
        }

        private void MostrarCanino(int index)
        {
            if (index >= 0 && index < caninos.Count)
            {
                var canino = caninos[index];

                // Asignar datos base (no modificables)
                NombreCanino.Text = canino.NombreCanino;
                RazaCanino.Text = $"Raza: {canino.RazaCanino}";

                // Asignar datos actualizables (verificar si hay valores en Preferences)
                var edadGuardada = Preferences.Get("EdadCanino", -1);
                var pesoGuardado = (decimal)Preferences.Get("PesoCanino", -1.0); // Conversión explícita a decimal
                var fotoGuardada = Preferences.Get("FotoCanino", string.Empty);

                // Usar valores de Preferences si existen, sino usar los de la base de datos
                EdadCanino.Text = $"Edad: {(edadGuardada != -1 ? edadGuardada : canino.EdadCanino)} años";
                PesoCanino.Text = $"Peso: {(pesoGuardado != -1.0m ? pesoGuardado : canino.PesoCanino)} kg"; // Formatear a decimal

                if (!string.IsNullOrEmpty(fotoGuardada))
                {
                    var fotoBytes = Convert.FromBase64String(fotoGuardada);
                    FotoCanino.Source = ImageSource.FromStream(() => new MemoryStream(fotoBytes));
                }
                else if (canino.FotoCanino != null && canino.FotoCanino.Length > 0)
                {
                    FotoCanino.Source = ImageSource.FromStream(() => new MemoryStream(canino.FotoCanino));
                }

                Preferences.Set("CaninoId", canino.IdCanino);
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
                await Navigation.PushAsync(new NotificacionPage(notificacion.Mensaje, notificacion.IdNotificacion));
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
            try
            {
                // Obtener el ID del canino desde Preferences que fue guardado en CargarDatosCanino
                int idCanino = Preferences.Get("CaninoId", 0);

                if (idCanino == 0)
                {
                    await DisplayAlert("Error", "No se pudo obtener la información del canino", "OK");
                    return;
                }

                // Navegar directamente a la página de crear historial
                await Navigation.PushAsync(new CrearHistorialPage(idCanino));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
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
            // Navegar a la página que muestra las recetas
            await Navigation.PushAsync(new RecetaPage());
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

        private async void OnSOSButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SOSPage());
        }

        // Método para manejar el clic en la tarjeta
        private void OnTarjetaClicked(object sender, EventArgs e)
        {
            if (caninos != null && caninos.Count > 0)
            {
                currentIndex = (currentIndex + 1) % caninos.Count;
                MostrarCanino(currentIndex);
            }
        }
    }
}