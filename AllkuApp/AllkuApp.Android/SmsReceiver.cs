using Android.App;
using Android.Content;
using Android.Telephony;
using System;
using System.Text.RegularExpressions;
using Xamarin.Essentials;
// Resolviendo ambigüedad con SmsMessage
using AndroidSmsMessage = Android.Telephony.SmsMessage;

namespace AllkuApp.Droid
{
    [BroadcastReceiver(Enabled = true, Label = "SMS Receiver", Exported = true)]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" }, Priority = (int)IntentFilterPriority.HighPriority)]
    public class SmsReceiver : BroadcastReceiver
    {
        // Expresión regular mejorada para capturar tanto el formato con espacios como sin ellos
        private static readonly Regex locationRegex = new Regex(@"http://maps\.google\.com/\?q=(-?\d+\.?\d*),(-?\d+\.?\d*)", RegexOptions.IgnoreCase);

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
                    ProcessSmsMessage(context, pdu);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🚨 Error procesando SMS: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private void ProcessSmsMessage(Context context, Java.Lang.Object pdu)
        {
            try
            {
                AndroidSmsMessage smsMessage;

                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
                {
                    // Método alternativo que no requiere JavaCast
                    var telephonyManager = (TelephonyManager)context.GetSystemService(Context.TelephonyService);
                    string format = telephonyManager?.PhoneType.ToString() ?? "";
                    smsMessage = AndroidSmsMessage.CreateFromPdu((byte[])pdu, format);
                }
                else
                {
                    smsMessage = AndroidSmsMessage.CreateFromPdu((byte[])pdu);
                }

                var senderNumber = smsMessage.OriginatingAddress;
                var messageBody = smsMessage.MessageBody;

                Console.WriteLine($"📩 SMS recibido de: {senderNumber}");
                Console.WriteLine($"📄 Contenido: {messageBody}");

                // Extraer coordenadas si el mensaje tiene un enlace de Google Maps
                var coordinates = ExtractCoordinates(messageBody);
                if (coordinates != null)
                {
                    Console.WriteLine($"📍 Coordenadas extraídas: Lat = {coordinates.Value.latitude}, Lng = {coordinates.Value.longitude}");
                    UpdateLocation(context, coordinates.Value.latitude, coordinates.Value.longitude);
                }
                else
                {
                    Console.WriteLine("⚠️ El mensaje no contiene coordenadas válidas.");
                    // Intentar buscar coordenadas directamente en el texto
                    var directMatch = Regex.Match(messageBody, @"(-?\d+\.?\d*),(-?\d+\.?\d*)");
                    if (directMatch.Success)
                    {
                        if (double.TryParse(directMatch.Groups[1].Value, out double lat) &&
                            double.TryParse(directMatch.Groups[2].Value, out double lng))
                        {
                            Console.WriteLine($"📍 Coordenadas extraídas directamente: Lat = {lat}, Lng = {lng}");
                            UpdateLocation(context, lat, lng);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🚨 Error al procesar SMS: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private (double latitude, double longitude)? ExtractCoordinates(string message)
        {
            try
            {
                var match = locationRegex.Match(message);
                if (!match.Success)
                {
                    Console.WriteLine("⚠️ No se encontró un enlace válido en el mensaje.");
                    return null;
                }

                if (double.TryParse(match.Groups[1].Value, out double latitude) &&
                    double.TryParse(match.Groups[2].Value, out double longitude))
                {
                    if (latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180)
                    {
                        return (latitude, longitude);
                    }
                    else
                    {
                        Console.WriteLine("❌ Coordenadas fuera del rango válido.");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Error al convertir las coordenadas.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🚨 Error extrayendo coordenadas: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            return null;
        }

        private void UpdateLocation(Context context, double latitude, double longitude)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    Console.WriteLine($"✅ Actualizando ubicación en la app: Lat = {latitude}, Lng = {longitude}");

                    // Verificar si MainActivity.Instance existe
                    if (MainActivity.Instance != null)
                    {
                        MainActivity.Instance.SetUbicacion(latitude, longitude);
                    }
                    else
                    {
                        Console.WriteLine("⚠️ MainActivity.Instance es nulo, enviando Intent alternativo");
                        // Alternativa: enviar un intent a la actividad principal
                        Intent intent = new Intent(context, typeof(MainActivity));
                        intent.PutExtra("latitude", latitude);
                        intent.PutExtra("longitude", longitude);
                        intent.AddFlags(ActivityFlags.NewTask);
                        context.StartActivity(intent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"🚨 Error actualizando ubicación: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                }
            });
        }
    }
}