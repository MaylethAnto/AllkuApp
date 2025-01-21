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

                // Reemplaza con la URL de tu API que genera y envía el token al correo
                var response = await client.PostAsync("https://allkuapi.sytes.net/api/Recuperacion/solicitar-recuperacion", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    // Aquí asumimos que la API te devuelve el token
                    string token = result.token;

                    // Enviar un mensaje de éxito al usuario
                    await DisplayAlert("Éxito",
                        $"Se ha enviado un enlace de recuperación a tu correo electrónico. Usa el token: {token}",
                        "OK");

                    // Navegar a la página donde el usuario ingresa el token
                    await Navigation.PushAsync(new ResetPasswordPage(token));

                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync(); // Obtén el contenido de la respuesta
                    await DisplayAlert("Error",
                        $"No se pudo procesar la solicitud. Detalles: {responseContent}",
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

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navegar de regreso a la página anterior
            await Navigation.PopAsync();
        }
    }
}
