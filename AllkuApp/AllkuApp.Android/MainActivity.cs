using Android.App;
using Android;
using Android.Content.PM;
using Android.OS;
using Android.Telephony;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AllkuApp.Servicios;
using AllkuApp.Vista;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AllkuApp;
using System;
using Xamarin.Essentials;
using AndroidX.AppCompat.App;
using Android.Content;

namespace AllkuApp.Droid
{
    [Activity(Label = "AllkuApp", Icon = "@mipmap/ic_launcher", Theme = "@style/SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity, IMainActivityService
    {
        const int RequestSmsPermissionId = 0;
        public static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            Xamarin.FormsGoogleMaps.Init(this, savedInstanceState);

            // Establecer la instancia antes de cargar la aplicación
            Instance = this;

            LoadApplication(new App());
            RequestSmsPermission();
            DependencyService.Register<IMainActivityService, MainActivity>();

            // Verificar si se inició desde un intent con coordenadas
            CheckForLocationIntent(Intent);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            // Manejar nuevos intents que puedan contener coordenadas
            CheckForLocationIntent(intent);
        }

        private void CheckForLocationIntent(Intent intent)
        {
            if (intent != null && intent.HasExtra("latitude") && intent.HasExtra("longitude"))
            {
                double latitude = intent.GetDoubleExtra("latitude", 0);
                double longitude = intent.GetDoubleExtra("longitude", 0);
                Console.WriteLine($"Recibidas coordenadas desde Intent: Lat = {latitude}, Lng = {longitude}");
                SetUbicacion(latitude, longitude);
            }
        }

        void RequestSmsPermission()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.SendSms) != Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReceiveSms) != Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadSms) != Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] {
                    Manifest.Permission.SendSms,
                    Manifest.Permission.ReceiveSms,
                    Manifest.Permission.ReadSms,
                    Manifest.Permission.AccessFineLocation
                }, RequestSmsPermissionId);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void SendSms(string phoneNumber, string message)
        {
            try
            {
                SmsManager smsManager = SmsManager.Default;
                smsManager.SendTextMessage(phoneNumber, null, message, null, null);
                Console.WriteLine($"SMS enviado a {phoneNumber}: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar SMS: {ex.Message}");
            }
        }

        // Método para actualizar la ubicación en la página GPSPage
        public void SetUbicacion(double latitud, double longitud)
        {
            Console.WriteLine($"SetUbicacion llamado con: Latitud = {latitud}, Longitud = {longitud}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (GPSPage.Instance != null)
                    {
                        Console.WriteLine("Actualizando mapa en GPSPage.Instance");
                        GPSPage.Instance.UpdateMap(latitud, longitud);
                    }
                    else
                    {
                        Console.WriteLine("GPSPage.Instance es nulo, intentando navegar a GPSPage");
                        // Si GPSPage.Instance es nulo, podemos intentar navegar a esa página
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            var navigationService = DependencyService.Get<INavigationService>();
                            if (navigationService != null)
                            {
                                await navigationService.NavigateToGPSPage(latitud, longitud);
                            }
                            else
                            {
                                // Almacenar las coordenadas para usarlas más tarde
                                App.LastReceivedLatitude = latitud;
                                App.LastReceivedLongitude = longitud;
                                App.HasPendingCoordinates = true;
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en SetUbicacion: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                }
            });
        }

        protected override void OnDestroy()
        {
            // No establecer Instance a null aquí, podría causar problemas con SmsReceiver
            // Instance = null;

            var smsService = DependencyService.Get<ISmsService>();
            smsService?.StopSmsListener();
            base.OnDestroy();
        }
    }
}