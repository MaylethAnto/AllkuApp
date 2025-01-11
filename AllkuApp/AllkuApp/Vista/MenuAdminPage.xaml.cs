using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuAdminPage : ContentPage
    {
        public MenuAdminPage()
        {
            InitializeComponent();
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navegar de regreso a la página anterior
            await Navigation.PopAsync();
        }
        private async void OnPaseadorClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistroPaseadorPage());
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            // Cerrar sesión y navegar a la página de inicio de sesión
            SecureStorage.Remove("user_token");
            SecureStorage.Remove("user_id");
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }

    }
}