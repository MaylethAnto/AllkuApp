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
            // Inicializar directamente con SplashPage sin NavigationPage
            MainPage = new SplashPage();

            // Registra tus páginas
            Routing.RegisterRoute("forgotpassword", typeof(ForgotPasswordPage));

            // Manejar excepciones globales
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
        }

        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogoutUser();
            Console.WriteLine($"Error no manejado en AppDomain: {e.ExceptionObject}");
        }

        private void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            LogoutUser();
            Console.WriteLine($"Error no manejado en Task: {e.Exception}");
        }

        private async void LogoutUser()
        {
            try
            {
                Application.Current.Properties["IsLoggedIn"] = false;
                Application.Current.Properties["UserToken"] = null;
                await Application.Current.SavePropertiesAsync();

                Device.BeginInvokeOnMainThread(() =>
                {
                    MainPage = new SplashPage(); // Sin NavigationPage
                });

                Console.WriteLine("Sesión cerrada debido a un error no controlado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante el cierre de sesión: {ex}");
            }
        }

        protected override void OnStart()
        {
            if (Application.Current.Properties.ContainsKey("AppClosedProperly"))
            {
                bool closedProperly = (bool)Application.Current.Properties["AppClosedProperly"];
                if (!closedProperly)
                {
                    LogoutUser();
                }
            }

            Application.Current.Properties["AppClosedProperly"] = false;
            Application.Current.SavePropertiesAsync();
        }

        protected override void OnSleep()
        {
            Application.Current.Properties["AppClosedProperly"] = true;
            Application.Current.SavePropertiesAsync();
        }

        protected override void OnResume()
        {
            Application.Current.Properties["AppClosedProperly"] = false;
            Application.Current.SavePropertiesAsync();
        }
    }
}
