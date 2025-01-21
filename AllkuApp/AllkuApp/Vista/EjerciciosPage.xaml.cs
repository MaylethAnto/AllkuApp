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

        private bool _isListaVacia;
        public bool IsListaVacia
        {
            get => _isListaVacia;
            set
            {
                _isListaVacia = value;
                OnPropertyChanged(nameof(IsListaVacia));
            }
        }
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public Command RefreshCommand { get; }


        public EjerciciosPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            Distancia = new ObservableCollection<DistanciaModel>();
            RefreshCommand = new Command(async () => await RefreshData());
            BindingContext = this;

        }

        private async Task RefreshData()
        {
            if (_isLoading) return;

            try
            {
                IsRefreshing = true;
                Distancia.Clear();
                await CargarPaseosFinalizados();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al refrescar datos: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
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
            try
            {
                Distancia.Clear();

                var idCanino = Preferences.Get("CaninoId", -1);
                if (idCanino == -1)
                {
                    await DisplayAlert("Error", "No se encontró el ID del canino.", "OK");
                    return;
                }

                var paseosFinalizados = await _apiService.ObtenerPaseosFinalizadosAsync(idCanino);

                if (paseosFinalizados?.Any() == true)
                {
                    // No convertir si las fechas ya están en UTC
                    var paseosOrdenados = paseosFinalizados
                        .OrderByDescending(p => p.FechaFin)
                        .Select(p => new DistanciaModel
                        {
                            FechaInicio = p.FechaInicio?.ToLocalTime() ?? DateTime.UtcNow.ToLocalTime(),
                            FechaFin = p.FechaFin?.ToLocalTime() ?? DateTime.UtcNow.ToLocalTime(),
                            DistanciaTotal = p.DistanciaKm ?? 0
                        });

                    foreach (var paseo in paseosOrdenados)
                    {
                        Distancia.Add(paseo);
                    }
                }

                // Actualizar el estado de IsListaVacia
                IsListaVacia = !Distancia.Any();
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

        private async void OnEliminarPaseoClicked(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.CommandParameter is DistanciaModel paseo)
            {
                var confirm = await DisplayAlert("Confirmar", "¿Estás seguro de que deseas eliminar este paseo?", "Sí", "No");
                if (confirm)
                {
                    Distancia.Remove(paseo);
                    await DisplayAlert("Éxito", "Paseo eliminado.", "OK");
                }
            }
        }
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}