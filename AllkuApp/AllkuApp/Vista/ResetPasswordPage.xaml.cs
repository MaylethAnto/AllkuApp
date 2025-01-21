using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace AllkuApp.Vista
{
    public partial class ResetPasswordPage : ContentPage
    {
        private string _token;

        public ResetPasswordPage(string token)
        {
            InitializeComponent();
            _token = token;
        }

        private async void OnResetPasswordClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(NewPasswordEntry.Text) || string.IsNullOrEmpty(ConfirmPasswordEntry.Text))
            {
                await DisplayAlert("Error", "Por favor ingresa todas las contraseñas", "OK");
                return;
            }

            if (NewPasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }

            try
            {
                var client = new HttpClient();
                var request = new
                {
                    Token = _token,
                    NuevaContrasena = NewPasswordEntry.Text
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Reemplaza con la URL de tu API para restablecer la contraseña
                var response = await client.PostAsync("https://allkuapi.sytes.net/api/Recuperacion/restablecer-contrasena", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Tu contraseña ha sido restablecida", "OK");

                    // Volver a la página de login
                    await Navigation.PopToRootAsync();
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"No se pudo restablecer la contraseña. Detalles: {responseContent}", "OK");
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Ocurrió un error al restablecer tu contraseña. Intenta más tarde", "OK");
            }
        }
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navegar de regreso a la página anterior
            await Navigation.PushAsync(new LoginPage());
        }
    }
}
