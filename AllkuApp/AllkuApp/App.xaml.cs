using AllkuApp.Vista;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AllkuApp
{
    public partial class App : Application
    {
       
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new SplashPage());
            // Registra tus páginas
            Routing.RegisterRoute("forgotpassword", typeof(ForgotPasswordPage));

          

            // Manejar excepciones globales
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
        }

        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogoutUser();
        }

        private void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved(); // Marcar la excepción como manejada
            LogoutUser();
        }

        private void LogoutUser()
        {

            MainPage = new NavigationPage(new SplashPage());

            // Limpia los datos de sesión
            Application.Current.Properties["IsLoggedIn"] = false;
            Application.Current.Properties["UserToken"] = null;
            Application.Current.SavePropertiesAsync();

            // Opcional: Log para diagnosticar errores
            Console.WriteLine("Sesión cerrada debido a un error no controlado.");

        }
        protected override void OnStart()
        {
            // Manejar cuando la aplicación pasa al fondo
            LogoutUser();
        }

        protected override void OnSleep()
        {

        }

        protected override void OnResume()
        {
        }
    }
}