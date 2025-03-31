using AllkuApp.Vista;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AllkuApp.Servicios
{
    // En la carpeta Servicios de tu proyecto .NET Standard (AllkuApp)
    public interface INavigationService
    {
        Task NavigateToGPSPage(double latitude, double longitude);
    }

    // Implementación en tu proyecto
    public class NavigationService : INavigationService
    {
        public async Task NavigateToGPSPage(double latitude, double longitude)
        {
            var gpsPage = new GPSPage();
            await App.Current.MainPage.Navigation.PushAsync(gpsPage);
            // Dar tiempo a la página para inicializarse
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () => {
                gpsPage.UpdateMap(latitude, longitude);
                return false; // No repetir el timer
            });
        }
    }
}
