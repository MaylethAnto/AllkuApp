using System.Net.Mail;
using System.Threading.Tasks;

namespace AllkuApp.Servicios
{
    public class EmailService
    {
        public async Task<bool> EnviarCorreoAsync(string correoDestino, string token)
        {
            var mensaje = new MimeKit.MimeMessage();
            mensaje.From.Add(new MimeKit.MailboxAddress("Allku App", "allkuapp@gmail.com"));
            mensaje.To.Add(new MimeKit.MailboxAddress("", correoDestino));
            mensaje.Subject = "Recuperación de Contraseña";
            mensaje.Body = new MimeKit.TextPart("plain")
            {
                Text = $"Este es tu token de recuperación: {token}\n\n" +
                                       "Ingresa este token en la aplicación para restablecer tu contraseña. \n\n" +
                                        "Si no solicitaste este cambio, ignora este mensaje."
            };

            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("allkuapp@gmail.com", "xmxj jkse pajh njnt");
                    await client.SendAsync(mensaje);
                    await client.DisconnectAsync(true);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}