using Pagina1.Servicios;
using System;
using System.Timers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Pagina1.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GPSPage : ContentPage
    {
        private string latitud;
        private string longitud;
        private Timer timer;

        public GPSPage()
        {
            InitializeComponent();
            RequestPermissions();
            InitializeMap();
        }

        // Método para solicitar permisos de ubicación
        private async void RequestPermissions()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
        }

        // Método para inicializar el mapa en La Carolina, Quito
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

        // Método para establecer la ubicación desde un mensaje SMS
        public void SetUbicacion(string lat, string lon)
        {
            latitud = lat;
            longitud = lon;
        }

        // Método para conectar al GPS y obtener la ubicación periódicamente
        private void OnConnectAndTrackClicked(object sender, EventArgs e)
        {
            var phoneNumber = "+593959020392"; // Reemplaza con el número del dispositivo GPS
            var connectMessage = "000"; // El comando para conectar el dispositivo GPS
            var trackMessage = "777"; // El comando para obtener la ubicación

            // Llama al método SendSms en MainActivity para conectar el dispositivo
            var mainActivity = DependencyService.Get<IMainActivityService>();
            mainActivity?.SendSms(phoneNumber, connectMessage);

            // Configura un Timer para enviar el comando de seguimiento periódicamente
            timer = new Timer(60000); // Intervalo de 60,000 milisegundos (1 minuto)
            timer.Elapsed += (s, args) =>
            {
                mainActivity?.SendSms(phoneNumber, trackMessage);
            };
            timer.Start();
        }

        // Método para obtener y actualizar la ubicación en el mapa
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