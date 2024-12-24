using Pagina1.Servicios;
using System;
using System.Timers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace Pagina1.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GPSPage : ContentPage
    {
        private Timer timer;

        public GPSPage()
        {
            InitializeComponent();
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

        private void OnConnectAndTrackClicked(object sender, EventArgs e)
        {
            var phoneNumber = "+593959020392";
            var connectMessage = "000";
            var trackMessage = "777";

            var mainActivity = DependencyService.Get<IMainActivityService>();
            mainActivity?.SendSms(phoneNumber, connectMessage);

            timer = new Timer(60000);
            timer.Elapsed += (s, args) =>
            {
                mainActivity?.SendSms(phoneNumber, trackMessage);
            };
            timer.Start();
        }

        public void UpdateMap(double latitude, double longitude)
        {
            var position = new Position(latitude, longitude);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(1)));

            var pin = new Pin
            {
                Type = PinType.Place,
                Position = position,
                Label = "GF-07 Tracker",
                Address = "Current Location"
            };
            map.Pins.Add(pin);
        }
    }
}