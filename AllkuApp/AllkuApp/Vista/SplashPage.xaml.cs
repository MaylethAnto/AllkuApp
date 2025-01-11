using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            StartAnimation();
        }

        private async void StartAnimation()
        {
            // Animar el logo (fade-in y scale)
            await LogoImage.FadeTo(1, 2000); // De opacidad 0 a 1 en 2 segundos
            await LogoImage.ScaleTo(1.2, 1000, Easing.CubicInOut); // Zoom in
            await LogoImage.ScaleTo(1, 1000, Easing.CubicInOut);   // Volver al tamaño original

            // Aparece el footer con el texto
            await FooterLabel.FadeTo(1, 1500, Easing.CubicInOut);

            // Esperar un momento y luego navegar a la página principal
            await Task.Delay(1000);
            await Navigation.PushAsync(new LoginPage());
        }
    }
}