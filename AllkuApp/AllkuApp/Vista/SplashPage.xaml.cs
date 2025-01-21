using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false); // Ocultar barra de navegación
            StartAnimation();
        }

        private async void StartAnimation()
        {
            try
            {
                // Animar el logo (fade-in y scale)
                await LogoImage.FadeTo(1, 2000); // De opacidad 0 a 1 en 2 segundos
                await LogoImage.ScaleTo(1.2, 1000, Easing.CubicInOut); // Zoom in
                await LogoImage.ScaleTo(1, 1000, Easing.CubicInOut);   // Volver al tamaño original

                // Aparece el footer con el texto
                await FooterLabel.FadeTo(1, 1500, Easing.CubicInOut);

                // Esperar un momento
                await Task.Delay(1000);

                // Configurar la nueva página principal con NavigationPage
                Device.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                });
            }
            catch (Exception ex)
            {
                // Manejo de errores durante la animación
                Console.WriteLine($"Error durante la animación del splash: {ex}");
                Device.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                });
            }
        }
    }
}