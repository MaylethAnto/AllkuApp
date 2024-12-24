using Android.App;
using Android;
using Android.Content.PM;
using Android.OS;
using Android.Telephony;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Pagina1.Servicios;
using Pagina1.Vista;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Pagina1.Droid
{
    [Activity(Label = "Pagina1", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
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

            // Registra la dependencia
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

        public void SetUbicacion(double latitud, double longitud)
        {
            // Lógica para actualizar la ubicación en la página GPSPage
            var mainPage = (App.Current.MainPage as NavigationPage)?.CurrentPage as GPSPage;
            mainPage?.UpdateMap(latitud, longitud);
        }
    }
}