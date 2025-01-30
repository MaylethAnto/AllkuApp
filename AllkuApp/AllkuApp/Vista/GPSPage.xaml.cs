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

        private void OnConnectClicked(object sender, EventArgs e)
        {
            var phoneNumber = "+593959020392";
            var connectMessage = "000";

            var mainActivity = DependencyService.Get<IMainActivityService>();
            mainActivity?.SendSms(phoneNumber, connectMessage);
        }

        private void OnTrackClicked(object sender, EventArgs e)
        {
            var phoneNumber = "+593959020392";
            var trackMessage = "777";

            var mainActivity = DependencyService.Get<IMainActivityService>();
            mainActivity?.SendSms(phoneNumber, trackMessage);

            timer = new Timer(60000);
            timer.Elapsed += (s, args) =>
            {
                mainActivity?.SendSms(phoneNumber, trackMessage);
            };
            timer.Start();
        }

        public void UpdateMap(double latitude, double longitude)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    Console.WriteLine($"UpdateMap called with: Latitude = {latitude}, Longitude = {longitude}");

                    // Limpiar pins existentes
                    map.Pins.Clear();

                    var position = new Position(latitude, longitude);

                    // Crear nuevo pin
                    var pin = new Pin
                    {
                        Type = PinType.Place,
                        Position = position,
                        Label = "GF-07 Tracker",
                        Address = $"Lat: {latitude}, Long: {longitude}"
                    };

                    // Agregar el nuevo pin
                    map.Pins.Add(pin);

                    // Mover el mapa a la nueva posición
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(1)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating map: {ex.Message}");
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
