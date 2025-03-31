using AllkuApp.Servicios;
using System;
using System.Timers;
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
        public static GPSPage Instance { get; private set; }

        public GPSPage()
        {
            InitializeComponent();
            Instance = this;
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
                await DisplayAlert("Permission Denied", "Location permission is required to display the map.", "OK");
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
            MostrarRecorrido();
        }

        private void MostrarRecorrido()
        {
            var latitudInicioStr = Preferences.Get("LatitudInicio", null);
            var longitudInicioStr = Preferences.Get("LongitudInicio", null);
            var latitudFinalStr = Preferences.Get("LatitudFinal", null);
            var longitudFinalStr = Preferences.Get("LongitudFinal", null);

            if (latitudInicioStr != null && longitudInicioStr != null && latitudFinalStr != null && longitudFinalStr != null)
            {
                if (double.TryParse(latitudInicioStr, out double latitudInicio) &&
                    double.TryParse(longitudInicioStr, out double longitudInicio) &&
                    double.TryParse(latitudFinalStr, out double latitudFinal) &&
                    double.TryParse(longitudFinalStr, out double longitudFinal))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            // Limpiar pins existentes
                            map.Pins.Clear();

                            var posicionInicio = new Position(latitudInicio, longitudInicio);
                            var posicionFinal = new Position(latitudFinal, longitudFinal);

                            // Crear pin de inicio
                            var pinInicio = new Pin
                            {
                                Type = PinType.Place,
                                Position = posicionInicio,
                                Label = "Inicio del Paseo",
                                Address = $"Lat: {latitudInicio}, Long: {longitudInicio}"
                            };

                            // Crear pin de final
                            var pinFinal = new Pin
                            {
                                Type = PinType.Place,
                                Position = posicionFinal,
                                Label = "Fin del Paseo",
                                Address = $"Lat: {latitudFinal}, Long: {longitudFinal}"
                            };

                            // Agregar los pins al mapa
                            map.Pins.Add(pinInicio);
                            map.Pins.Add(pinFinal);

                            // Crear una línea para representar el recorrido
                            var recorrido = new Polyline
                            {
                                StrokeColor = Color.Blue,
                                StrokeWidth = 5
                            };

                            recorrido.Positions.Add(posicionInicio);
                            recorrido.Positions.Add(posicionFinal);

                            // Agregar la línea al mapa
                            map.Polylines.Add(recorrido);

                            // Mover el mapa para mostrar el recorrido completo
                            var bounds = new Bounds(posicionInicio, posicionFinal);
                            map.MoveToRegion(MapSpan.FromBounds(bounds));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error mostrando recorrido en el mapa: {ex.Message}");
                        }
                    });
                }
            }
        }

        // Método para actualizar la ubicación en el mapa
        public void UpdateMap(double latitud, double longitud)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    // Limpiar pins existentes
                    map.Pins.Clear();

                    var nuevaPosicion = new Position(latitud, longitud);

                    // Crear un nuevo pin en la nueva posición
                    var pinNuevo = new Pin
                    {
                        Type = PinType.Place,
                        Position = nuevaPosicion,
                        Label = "Nueva Ubicación",
                        Address = $"Lat: {latitud}, Long: {longitud}"
                    };

                    // Agregar el nuevo pin al mapa
                    map.Pins.Add(pinNuevo);

                    // Mover el mapa a la nueva posición
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