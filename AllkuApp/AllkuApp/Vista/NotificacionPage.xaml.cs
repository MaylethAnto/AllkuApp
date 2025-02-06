using AllkuApp.Servicios;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificacionPage : ContentPage
    {
        private int _idNotificacion;
        private readonly ApiService _apiService;

        public NotificacionPage(string message, int idNotificacion)
        {
            InitializeComponent();
            // Set the notification message and paseador's number
            NotificationMessage.Text = message;
            _idNotificacion = idNotificacion;
        }
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navegar de regreso a la página anterior
            await Navigation.PopAsync();
        }

    }
}