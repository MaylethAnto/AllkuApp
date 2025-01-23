using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AllkuApp.Servicios;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AllkuApp.Vista
{
    public partial class ForgotPasswordPage : ContentPage
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }

        private async void OnRecuperarClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UsernameEntry.Text) || string.IsNullOrEmpty(EmailEntry.Text))
            {
                await DisplayAlert("Error", "Por favor ingresa todos los campos", "OK");
                return;
            }

            if (!IsValidEmail(EmailEntry.Text))
            {
                await DisplayAlert("Error", "Ingresa un correo válido", "OK");
                return;
            }

            try
            {
                var token = Guid.NewGuid().ToString();
                var emailService = new EmailService();

                if (await emailService.EnviarCorreoAsync(EmailEntry.Text, token))
                {
                    await DisplayAlert("Éxito", "Correo enviado", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Correo no registrado", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Problema: {ex.Message}", "OK");
            }
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> IsValidUserAndEmail(string username, string email)
        {
            try
            {
                var client = new HttpClient();
                var request = new
                {
                    NombreUsuario = username,
                    Correo = email
                };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var verificacionResponse = await client.PostAsync("https://allkuapi.sytes.net/api/Usuarios/verificar-correo", content);

                if (verificacionResponse.IsSuccessStatusCode)
                {
                    // Si la respuesta es exitosa, el correo coincide con el usuario
                    return true;
                }
                else
                {
                    // Si la respuesta no es exitosa, significa que el correo no corresponde al usuario
                    await DisplayAlert("Error", "El correo no coincide con el usuario", "OK");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al verificar el correo: {ex.Message}", "OK");
                return false;
            }
        }
    }
}