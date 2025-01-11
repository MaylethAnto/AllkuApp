using Android.App;
using Android.Content;
using Android.Telephony;
using System;
using System.Text.RegularExpressions;
using Xamarin.Essentials;

namespace AllkuApp.Droid
{
    [BroadcastReceiver(Enabled = true, Label = "SMS Receiver", Exported = true)]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" }, Priority = (int)IntentFilterPriority.HighPriority)]
    public class SmsReceiver : BroadcastReceiver
    {
        private const string GPS_PHONE_NUMBER = "+593959020392";
        // Nueva expresión regular que coincide con el formato específico del GF-07
        private static readonly Regex locationRegex = new Regex(@"http://maps\.google\.com/\?q=(-?\d+\.\d+),(-?\d+\.\d+)", RegexOptions.IgnoreCase);

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

            // Verifica si el mensaje viene del número del GPS
            if (!smsMessage.OriginatingAddress.Contains(GPS_PHONE_NUMBER)) return;

            var messageBody = smsMessage.MessageBody;
            Console.WriteLine($"Received message: {messageBody}"); // Debug log

            var coordinates = ExtractCoordinates(messageBody);
            if (coordinates != null)
            {
                Console.WriteLine($"Coordinates extracted: Latitude = {coordinates.Value.latitude}, Longitude = {coordinates.Value.longitude}");
                UpdateLocation(coordinates.Value.latitude, coordinates.Value.longitude);
            }
            else
            {
                Console.WriteLine("Failed to extract coordinates from message");
            }
        }

        private (double latitude, double longitude)? ExtractCoordinates(string message)
        {
            try
            {
                var match = locationRegex.Match(message);
                if (!match.Success)
                {
                    Console.WriteLine("No match found in message");
                    return null;
                }

                var latitude = double.Parse(match.Groups[1].Value);
                var longitude = double.Parse(match.Groups[2].Value);

                Console.WriteLine($"Parsed coordinates: Lat={latitude}, Long={longitude}");

                // Validar que las coordenadas están en rangos válidos
                if (latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180)
                {
                    return (latitude, longitude);
                }
                else
                {
                    Console.WriteLine("Coordinates out of valid range");
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
                    Console.WriteLine($"Updating location: Latitude = {latitude}, Longitude = {longitude}");
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