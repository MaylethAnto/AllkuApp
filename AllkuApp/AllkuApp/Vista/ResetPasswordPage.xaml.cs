using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace AllkuApp.Vista
{
    public partial class ResetPasswordPage : ContentPage
    {
        public ResetPasswordPage()
        {
            InitializeComponent();
        }

        private async void OnResetPasswordClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TokenEntry.Text) ||
                string.IsNullOrEmpty(NewPasswordEntry.Text) ||
                string.IsNullOrEmpty(ConfirmPasswordEntry.Text))
            {
                await DisplayAlert("Error", "Por favor completa todos los campos", "OK");
                return;
            }

            if (NewPasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }

            // Validar que el token ingresado sea correcto
            if (!IsValidToken(TokenEntry.Text))
            {
                await DisplayAlert("Error", "El token de recuperación es incorrecto", "OK");
                return;
            }

            try
            {
                var client = new HttpClient();
                var request = new
                {
                    Token = TokenEntry.Text,
                    NuevaContrasena = NewPasswordEntry.Text
                };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://allkuapi.sytes.net/api/Recuperacion/reset-password", content);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Contraseña restablecida correctamente", "OK");
                    await Navigation.PopToRootAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"No se pudo restablecer: {errorContent}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Problema al restablecer: {ex.Message}", "OK");
            }
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }

        private bool IsValidToken(string token)
        {
            // Implementa la lógica de validación del token
            // Por ejemplo, hacer una llamada al backend para verificar si el token es válido
            return token == "valid-token";
        }
    }
}