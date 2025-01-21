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

namespace AllkuApp.Droid
{
    [Activity(Label = "AllkuApp", Icon = "@mipmap/ic_launcher", Theme = "@style/SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity, IMainActivityService
    {
        const int RequestSmsPermissionId = 0;
        public static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            Xamarin.FormsGoogleMaps.Init(this, savedInstanceState);
            LoadApplication(new App());

            Instance = this;

            RequestSmsPermission();
            DependencyService.Register<IMainActivityService, MainActivity>();
        }

        void RequestSmsPermission()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.SendSms) != Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReceiveSms) != Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadSms) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] {
                    Manifest.Permission.SendSms,
                    Manifest.Permission.ReceiveSms,
                    Manifest.Permission.ReadSms
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
            SmsManager smsManager = SmsManager.Default;
            smsManager.SendTextMessage(phoneNumber, null, message, null, null);
        }

        // Método para actualizar la ubicación en la página GPSPage
        public void SetUbicacion(double latitud, double longitud)
        {
            Console.WriteLine($"SetUbicacion called with: Latitude = {latitud}, Longitude = {longitud}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    GPSPage.Instance?.UpdateMap(latitud, longitud);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in SetUbicacion: {ex.Message}");
                }
            });
        }
    }
}
