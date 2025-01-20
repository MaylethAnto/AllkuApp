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
            _numeroPaseador = numeroPaseador;
            _idNotificacion = idNotificacion;
        }
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navegar de regreso a la página anterior
            await Navigation.PopAsync();
        }

    }
}