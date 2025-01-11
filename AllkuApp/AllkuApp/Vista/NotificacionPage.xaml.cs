using AllkuApp.Servicios;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificacionPage : ContentPage
    {
        private string _numeroPaseador;
        private int _idNotificacion;
        private readonly ApiService _apiService;

        public NotificacionPage(string message, string numeroPaseador, int idNotificacion)
        {
            InitializeComponent();
            // Set the notification message and paseador's number
            NotificationMessage.Text = message;
            PaseadorNumber.Text = numeroPaseador;
            _numeroPaseador = numeroPaseador;
            _idNotificacion = idNotificacion;
        }
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navegar de regreso a la página anterior
            await Navigation.PopAsync();
        }
        private async void OnPaseadorNumberTapped(object sender, EventArgs e)
        {
            // Open WhatsApp with the paseador's number
            Device.OpenUri(new Uri($"https://wa.me/+593{_numeroPaseador}"));

            // Marcar la notificación como leída
            await _apiService.MarcarNotificacionComoLeidaAsync(_idNotificacion);
        }
    }
}