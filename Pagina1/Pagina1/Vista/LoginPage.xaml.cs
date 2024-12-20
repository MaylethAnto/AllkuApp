﻿using System;
using Xamarin.Forms;
using System.Net.Mail;
using Pagina1.Vista;
using Pagina1.Servicios; 
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace Pagina1.Vista
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginService _loginService;
        private readonly AuthService _authService;

        public LoginPage()
        {
            InitializeComponent();
            _loginService = new LoginService();  // Para login
            _authService = new AuthService();    // Para registro
        }

        // Acción cuando se presiona el botón de Login
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string nombreUsuario = usernameEntry.Text;
            string contrasena = passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
            {
                statusLabel.Text = "Por favor, ingresa tu usuario y contraseña.";
                return;
            }

            try
            {
                var response = await _loginService.LoginUsuarioAsync(nombreUsuario, contrasena);

                if (response.Mensaje == "Login exitoso")
                {
                    var rol = response.Rol;

                    // Redirigir según el rol
                    switch (rol)
                    {
                        case "Administrador":
                            await Navigation.PushAsync(new AdminPage());
                            break;

                        case "Dueño":
                            bool esPrimeraVez = await VerificarPrimeraVez(nombreUsuario);
                            if (esPrimeraVez)
                            {
                                await Navigation.PushAsync(new RegistroMascotaPage());
                            }
                            else
                            {
                                await Navigation.PushAsync(new MainPage());
                            }
                            break;

                        case "Paseador":
                            await Navigation.PushAsync(new PaseadoresPage());
                            break;

                        default:
                            statusLabel.TextColor = Color.Red;
                            statusLabel.Text = "Rol desconocido, contacte al administrador.";
                            break;
                    }
                }
                else
                {
                    statusLabel.TextColor = Color.Red;
                    statusLabel.Text = response.Mensaje;
                }
            }
            catch (Exception ex)
            {
                statusLabel.TextColor = Color.Red;
                statusLabel.Text = "Error al iniciar sesión: " + ex.Message;
            }
        }


        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            // Navigate to the RegisterPage
            await Navigation.PushAsync(new RegisterPage());
        }

        // Método simulado para verificar si es la primera vez que inicia sesión
        private async Task<bool> VerificarPrimeraVez(string nombreUsuario)
        {
            // Lógica simulada: podrías consultar una API para validar este estado
            return await Task.FromResult(true); // Cambiar según la lógica real
        }

    }
}
