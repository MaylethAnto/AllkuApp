using Pagina1.Vista;
using System;
using System.IO;
using Xamarin.Forms;

namespace Pagina1
{
    public partial class App : Application
    {
       
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new SplashPage());
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}