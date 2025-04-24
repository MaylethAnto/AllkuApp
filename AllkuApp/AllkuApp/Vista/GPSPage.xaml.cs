using AllkuApp.Modelo;
using AllkuApp.Servicios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GPSPage : ContentPage
    {
        private Timer timer;
        private readonly ApiService _apiService;
        public static GPSPage Instance { get; private set; }

        public GPSPage()
        {
            InitializeComponent();
            Instance = this;
            _apiService = new ApiService();
            RequestPermissions();
        }

        private async void RequestPermissions()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            if (status == PermissionStatus.Granted)
            {
                InitializeMap();
            }
            else
            {
                await DisplayAlert("Permiso denegado", "Se requiere permiso de ubicación para mostrar el mapa.", "OK");
            }
        }

        private void InitializeMap()
        {
            var laCarolina = new Position(-0.182884, -78.484499);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(laCarolina, Distance.FromKilometers(1)));

            var pin = new Pin
            {
                Type = PinType.Place,
                Position = laCarolina,
                Label = "La Carolina",
                Address = "Quito, Ecuador"
            };
            map.Pins.Add(pin);
        }

        private void OnMostrarRecorridoClicked(object sender, EventArgs e)
        {
            MostrarUltimoRecorridoCanino();
        }

        private async void MostrarUltimoRecorridoCanino()
        {
            try
            {
                var idCanino = Preferences.Get("CaninoId", -1);
                if (idCanino == -1)
                {
                    await DisplayAlert("Error", "No se encontró el ID del canino.", "OK");
                    return;
                }

                var paseos = await ObtenerPaseosFinalizadosAsync(idCanino);
                if (paseos == null || !paseos.Any())
                {
                    await DisplayAlert("Información", "No hay paseos registrados para este canino", "OK");
                    return;
                }

                var ultimoPaseo = paseos.OrderByDescending(p => p.FechaInicio).FirstOrDefault();

                if (ultimoPaseo == null ||
                    !ultimoPaseo.LatitudInicio.HasValue ||
                    !ultimoPaseo.LongitudInicio.HasValue ||
                    !ultimoPaseo.LatitudFin.HasValue ||
                    !ultimoPaseo.LongitudFin.HasValue)
                {
                    await DisplayAlert("Información", "El último paseo no tiene coordenadas completas registradas.", "OK");
                    return;
                }

                double latitudInicio = ultimoPaseo.LatitudInicio.Value;
                double longitudInicio = ultimoPaseo.LongitudInicio.Value;
                double latitudFinal = ultimoPaseo.LatitudFin.Value;
                double longitudFinal = ultimoPaseo.LongitudFin.Value;


                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        map.Pins.Clear();
                        map.Polylines?.Clear();

                        var posicionInicio = new Position(latitudInicio, longitudInicio);
                        var posicionFinal = new Position(latitudFinal, longitudFinal);

                        var pinInicio = new Pin
                        {
                            Type = PinType.Place,
                            Position = posicionInicio,
                            Label = "📍 Punto A (Inicio)",
                            Address = $"Lat: {latitudInicio}, Long: {longitudInicio}"
                        };

                        var pinFinal = new Pin
                        {
                            Type = PinType.Place,
                            Position = posicionFinal,
                            Label = "🏁 Punto B (Fin)",
                            Address = $"Lat: {latitudFinal}, Long: {longitudFinal}"
                        };

                        map.Pins.Add(pinInicio);
                        map.Pins.Add(pinFinal);

                        var recorrido = new Polyline
                        {
                            StrokeColor = Color.FromHex("#FF7F50"), // Color coral
                            StrokeWidth = 6
                        };
                        recorrido.Positions.Add(posicionInicio);
                        recorrido.Positions.Add(posicionFinal);

                        map.Polylines.Add(recorrido);

                        map.MoveToRegion(MapSpan.FromCenterAndRadius(
                            new Position((latitudInicio + latitudFinal) / 2, (longitudInicio + longitudFinal) / 2),
                            Distance.FromMeters(500))
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error mostrando recorrido en el mapa: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                await DisplayAlert("Error", "Ocurrió un error al cargar el recorrido", "OK");
            }
        }

        private async Task<List<PaseoModel>> ObtenerPaseosFinalizadosAsync(int idCanino)
        {
            try
            {
                return await _apiService.ObtenerPaseosFinalizadosAsync(idCanino);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener paseos: {ex.Message}");
                return new List<PaseoModel>();
            }
        }

        public void UpdateMap(double latitud, double longitud)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    map.Pins.Clear();

                    var nuevaPosicion = new Position(latitud, longitud);
                    var pinNuevo = new Pin
                    {
                        Type = PinType.Place,
                        Position = nuevaPosicion,
                        Label = "Nueva Ubicación",
                        Address = $"Lat: {latitud}, Long: {longitud}"
                    };

                    map.Pins.Add(pinNuevo);
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(nuevaPosicion, Distance.FromKilometers(1)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error actualizando mapa: {ex.Message}");
                }
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            timer?.Stop();
            timer?.Dispose();
        }
    }
}
