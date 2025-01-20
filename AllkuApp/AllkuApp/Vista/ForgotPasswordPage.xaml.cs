using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
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

            try
            {
                var client = new HttpClient();
                var request = new
                {
                    NombreUsuario = UsernameEntry.Text,
                    Correo = EmailEntry.Text
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Reemplaza con la URL de tu API
                var response = await client.PostAsync("https://allkuapi.sytes.net/api/Recuperacion/solicitar-recuperacion", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    await DisplayAlert("Éxito",
                        "Se ha enviado un enlace de recuperación a tu correo electrónico",
                        "OK");

                    // Volver a la página de login
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error",
                        "No se pudo procesar la solicitud. Por favor, verifica tus datos",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error",
                    "Ocurrió un error al procesar tu solicitud. Intenta más tarde",
                    "OK");
            }
        }

        private async void OnVolverClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}