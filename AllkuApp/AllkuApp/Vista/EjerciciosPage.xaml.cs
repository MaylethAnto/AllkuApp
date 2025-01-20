using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading.Tasks;
using AllkuApp.Servicios;
using Xamarin.Essentials;
using AllkuApp.Modelo;
using System.Linq;
using System.Diagnostics;
using System.Net.Http;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EjerciciosPage : ContentPage
    {
        private bool _isFirstLoad = true;
        private readonly bool _dataLoaded = false;
        public ObservableCollection<DistanciaModel> Distancia { get; set; } = new ObservableCollection<DistanciaModel>();

        private readonly ApiService _apiService;
        private bool _isLoading;

        public EjerciciosPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            Distancia = new ObservableCollection<DistanciaModel>();
            BindingContext = this;

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_isFirstLoad)  // Solo cargar la primera vez
            {
                await CargarPaseosFinalizados();
                _isFirstLoad = false;
            }
        }

        private async Task CargarPaseosFinalizados()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                var idCanino = Preferences.Get("CaninoId", -1);
                if (idCanino == -1)
                {
                    await DisplayAlert("Error", "No se encontró el ID del canino.", "OK");
                    return;
                }

                var paseosFinalizados = await _apiService.ObtenerPaseosFinalizadosAsync(idCanino);
                Debug.WriteLine($"Número de paseos recibidos: {paseosFinalizados?.Count ?? 0}");

                if (paseosFinalizados?.Any() == true)
                {
                    // Definir el TimeZoneInfo para Ecuador (UTC-5)
                    var ecuadorZone = TimeZoneInfo.CreateCustomTimeZone(
                        "Ecuador Time",
                        new TimeSpan(-5, 0, 0),
                        "Ecuador Time",
                        "Ecuador Time");

                    foreach (var paseo in paseosFinalizados)
                    {
                        // Convertir las fechas a hora de Ecuador
                        var fechaInicioEcuador = TimeZoneInfo.ConvertTimeFromUtc(
                            paseo.FechaInicio?.ToUniversalTime() ?? DateTime.UtcNow,
                            ecuadorZone);

                        var fechaFinEcuador = TimeZoneInfo.ConvertTimeFromUtc(
                            paseo.FechaFin?.ToUniversalTime() ?? DateTime.UtcNow,
                            ecuadorZone);

                        var nuevoModelo = new DistanciaModel
                        {
                            FechaInicio = fechaInicioEcuador,
                            FechaFin = fechaFinEcuador,
                            DistanciaTotal = paseo.DistanciaKm ?? 0
                        };

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Distancia.Add(nuevoModelo);
                        });
                    }
                }
                else
                {
                    await DisplayAlert("Información", "No hay paseos finalizados para mostrar.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Hubo un error al cargar los paseos: {ex.Message}", "OK");
                Debug.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }


        // Asegúrate de desuscribirte cuando la página se destruya
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<object>(this, "PaseoFinalizado");
        }


        private async void OnElegirPaseadorClicked(object sender, EventArgs e)
        {
            try
            {
                // Obtener paseadores disponibles
                var paseadoresDisponibles = await _apiService.GetPaseadoresDisponiblesAsync();
                if (paseadoresDisponibles?.Count > 0)
                {
                    var paseadorPage = new PaseadorSelectionPage(paseadoresDisponibles);
                    paseadorPage.PaseadorSeleccionado += async (s, paseador) => {
                        // Enviar solicitud al paseador
                        var idCanino = Preferences.Get("CaninoId", -1);
                        if (idCanino != -1)
                        {
                            var resultado = await _apiService.EnviarSolicitudPaseoAsync(new SolicitudPaseoDto
                            {
                                IdCanino = idCanino,
                                CedulaPaseador = paseador.CedulaPaseador
                            });

                            if (resultado)
                            {
                                await DisplayAlert("Éxito", "Solicitud enviada al paseador.", "OK");
                            }
                            else
                            {
                                await DisplayAlert("Error", "No se pudo enviar la solicitud.", "OK");
                            }
                        }
                    };
                    await Navigation.PushModalAsync(paseadorPage);
                }
                else
                {
                    await DisplayAlert("Info", "No hay paseadores disponibles en este momento.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "No se pudieron cargar los paseadores disponibles.", "OK");
            }
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}