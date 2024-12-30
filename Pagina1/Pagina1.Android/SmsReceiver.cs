using Android.App;
using Android.Content;
using Android.Telephony;
using Xamarin.Essentials;
using System.Text.RegularExpressions;
using System;

namespace Pagina1.Droid
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" })]
    public class SmsReceiver : BroadcastReceiver
    {
        private const string GPS_PHONE_NUMBER = "+593959020392";
        private static readonly Regex locationRegex = new Regex(@"lat:([0-9.-]+).*long:([0-9.-]+)", RegexOptions.IgnoreCase);

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != "android.provider.Telephony.SMS_RECEIVED") return;

            var bundle = intent.Extras;
            if (bundle == null) return;

            try
            {
                var pdus = (Java.Lang.Object[])bundle.Get("pdus");
                if (pdus == null) return;

                foreach (var pdu in pdus)
                {
                    ProcessSmsMessage(pdu);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing SMS: {ex.Message}");
            }
        }

        private void ProcessSmsMessage(Java.Lang.Object pdu)
        {
            var bytes = (byte[])(pdu);
            var smsMessage = Android.Telephony.SmsMessage.CreateFromPdu(bytes);

            if (!smsMessage.OriginatingAddress.Contains(GPS_PHONE_NUMBER)) return;

            var coordinates = ExtractCoordinates(smsMessage.MessageBody);
            if (coordinates != null)
            {
                UpdateLocation(coordinates.Value.latitude, coordinates.Value.longitude);
            }
        }

        private (double latitude, double longitude)? ExtractCoordinates(string message)
        {
            var match = locationRegex.Match(message);
            if (!match.Success) return null;

            try
            {
                var latitude = double.Parse(match.Groups[1].Value);
                var longitude = double.Parse(match.Groups[2].Value);

                // Validar que las coordenadas están en rangos válidos
                if (latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180)
                {
                    return (latitude, longitude);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing coordinates: {ex.Message}");
            }

            return null;
        }

        private void UpdateLocation(double latitude, double longitude)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    MainActivity.Instance?.SetUbicacion(latitude, longitude);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating location: {ex.Message}");
                }
            });
        }
    }
}