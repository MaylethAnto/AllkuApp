using AllkuApp.Vista;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Diagnostics;
using System.Threading;

namespace AllkuApp
{
    public partial class App : Application
    {
        private bool isHandlingException = false;
        private static readonly SemaphoreSlim _logoutSemaphore = new SemaphoreSlim(1, 1);

        public App()
        {
            InitializeComponent();
            MainPage = new SplashPage();
            Routing.RegisterRoute("forgotpassword", typeof(ForgotPasswordPage));

            // Mejorar el manejo de excepciones
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
        }

        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            HandleException(exception);
            Debug.WriteLine($"Error no manejado en AppDomain: {exception}");
        }

        private void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            HandleException(e.Exception);
            Debug.WriteLine($"Error no manejado en Task: {e.Exception}");
        }

        private void HandleException(Exception ex)
        {
            if (isHandlingException) return;

            try
            {
                isHandlingException = true;

                // Forzar cierre de sesión en cualquier excepción no manejada
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await ForceLogoutUser();
                });
            }
            finally
            {
                isHandlingException = false;
            }
        }

        private async Task ForceLogoutUser()
        {
            try
            {
                // Usar semáforo para evitar múltiples cierres de sesión simultáneos
                await _logoutSemaphore.WaitAsync();

                // Limpiar todas las propiedades de sesión
                if (Application.Current.Properties.ContainsKey("IsLoggedIn"))
                {
                    Application.Current.Properties["IsLoggedIn"] = false;
                }
                if (Application.Current.Properties.ContainsKey("UserToken"))
                {
                    Application.Current.Properties["UserToken"] = null;
                }

                // Agregar una marca de tiempo del último cierre de sesión
                Application.Current.Properties["LastLogout"] = DateTime.UtcNow.ToString();

                // Forzar el guardado de propiedades
                await Application.Current.SavePropertiesAsync();

                // Limpiar el stack de navegación y volver a SplashPage
                Device.BeginInvokeOnMainThread(() =>
                {
                    MainPage = new SplashPage();
                });

                Debug.WriteLine("Sesión forzada a cerrar debido a un error.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error crítico durante el cierre de sesión forzado: {ex}");
                // En caso de error extremo, intentar limpiar todo
                Application.Current.Properties.Clear();
                await Application.Current.SavePropertiesAsync();
            }
            finally
            {
                _logoutSemaphore.Release();
            }
        }

        protected override async void OnStart()
        {
            try
            {
                // Verificar si la app se cerró correctamente
                if (Application.Current.Properties.ContainsKey("AppClosedProperly"))
                {
                    bool closedProperly = (bool)Application.Current.Properties["AppClosedProperly"];
                    if (!closedProperly)
                    {
                        await ForceLogoutUser();
                    }
                }

                Application.Current.Properties["AppClosedProperly"] = false;
                await Application.Current.SavePropertiesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnStart: {ex}");
                await ForceLogoutUser();
            }
        }

        protected override async void OnSleep()
        {
            try
            {
                Application.Current.Properties["AppClosedProperly"] = true;
                await Application.Current.SavePropertiesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnSleep: {ex}");
            }
        }

        protected override async void OnResume()
        {
            try
            {
                Application.Current.Properties["AppClosedProperly"] = false;
                await Application.Current.SavePropertiesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnResume: {ex}");
                await ForceLogoutUser();
            }
        }
    }
}