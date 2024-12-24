using Xamarin.Forms;
using Xamarin.Essentials;
using System;

namespace Pagina1.Vista
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        private void OnAddPetClicked(object sender, EventArgs e)
        {
            // Navegar a la página para agregar mascota
            Navigation.PushAsync(new RegistroMascotaPage());
        }

        private void OnPerfilClicked(object sender, EventArgs e)
        {
            // Navegar a la página de perfil
            Navigation.PushAsync(new PerfilPage());
        }

        private void OnEjerciciosClicked(object sender, EventArgs e)
        {
            // Navegar a la página de ejercicios
            Navigation.PushAsync(new EjerciciosPage());
        }

        private void OnRecetasClicked(object sender, EventArgs e)
        {
            // Navegar a la página de recetas
            Navigation.PushAsync(new RecetasPage());
        }

        private void OnGPSClicked(object sender, EventArgs e)
        {
            // Navegar a la página de GPS
            Navigation.PushAsync(new GPSPage());
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