using Android.App;
using Android.Content;
using Android.Telephony;

namespace Pagina1.Droid
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" })]
    public class SmsReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var bundle = intent.Extras;
            if (bundle != null)
            {
                var pdus = (Java.Lang.Object[])bundle.Get("pdus");
                if (pdus != null)
                {
                    foreach (var pdu in pdus)
                    {
                        var bytes = (byte[])(pdu); // Conversión correcta a byte[]
                        var smsMessage = SmsMessage.CreateFromPdu(bytes); // Crear mensaje SMS desde el PDU

                        var mensaje = smsMessage.MessageBody; // Cuerpo del mensaje
                        var remitente = smsMessage.OriginatingAddress; // Número del remitente

                        // Procesa el mensaje (puedes filtrar por el remitente del GPS, si es necesario)
                        if (remitente.Contains("+593959020392")) // Reemplaza "NUMERO_GPS" con el número real
                        {
                            string[] coordenadas = ExtraerCoordenadas(mensaje); // Método para extraer coordenadas
                            if (coordenadas != null)
                            {
                                double latitud = double.Parse(coordenadas[0]);
                                double longitud = double.Parse(coordenadas[1]);

                                // Llama al método para actualizar las coordenadas en MainActivity
                                MainActivity.Instance.SetUbicacion(latitud, longitud);
                            }
                        }
                    }
                }
            }
        }

        private string[] ExtraerCoordenadas(string mensaje)
        {
            // Asume que el mensaje contiene algo como: "Lat: XX.XXXX, Long: YY.YYYY"
            var partes = mensaje.Split(',');
            if (partes.Length == 2)
            {
                string latitud = partes[0].Split(':')[1].Trim();
                string longitud = partes[1].Split(':')[1].Trim(); 
                return new[] { latitud, longitud };
            }
            return null;
        }
    }
}