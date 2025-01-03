﻿using Pagina1.Dtos;
using Pagina1.Servicios;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Text.RegularExpressions;
using System.Linq;

namespace Pagina1.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegistroPaseadorPage : ContentPage
    {
        private readonly AuthService _authService;
        private readonly string[] validDomains = {
            "gmail.com", "outlook.com", "yahoo.com", "hotmail.com",
            "edu.ec", "edu.mx", "edu.ar", "edu.cl", "edu.pe", "edu.br", "edu.ve",
            "edu.bo", "edu.uy", "edu.cr", "edu.pa", "edu.ni", "edu.hn", "edu.sv",
            "edu.gt", "edu.do", "edu.pr"
        };

        public RegistroPaseadorPage()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarCampos()) return;

                var registerDto = new RegistrarUsuarioDto
                {
                    Cedula = CedulaEntry.Text.Trim(),
                    Nombre = NombreEntry.Text.Trim(),
                    Usuario = UsuarioEntry.Text.Trim(),
                    Correo = CorreoEntry.Text.Trim(),
                    Contrasena = ContraseñaEntry.Text,
                    Rol = "Paseador",
                    Direccion = DireccionEntry.Text.Trim(),
                    Celular = CelularEntry.Text.Trim()
                };

                Debug.WriteLine("Enviando datos al servicio de registro de paseador");
                var result = await _authService.RegistrarUsuarioAsync(registerDto);

                if (result)
                {
                    await DisplayAlert("Éxito", "¡Paseador registrado correctamente!", "Aceptar");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo registrar el paseador. Intente nuevamente.", "Aceptar");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Aceptar");
            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(CedulaEntry.Text) ||
                string.IsNullOrWhiteSpace(NombreEntry.Text) ||
                string.IsNullOrWhiteSpace(UsuarioEntry.Text) ||
                string.IsNullOrWhiteSpace(CorreoEntry.Text) ||
                string.IsNullOrWhiteSpace(ContraseñaEntry.Text) ||
                string.IsNullOrWhiteSpace(ConfirmarContraseñaEntry.Text) ||
                string.IsNullOrWhiteSpace(DireccionEntry.Text) ||
                string.IsNullOrWhiteSpace(CelularEntry.Text))
            {
                DisplayAlert("Error", "Por favor, complete todos los campos requeridos", "OK");
                return false;
            }

            if (CedulaEntry.Text.Length != 10 || !CedulaEntry.Text.All(char.IsDigit))
            {
                DisplayAlert("Error", "La cédula debe tener 10 dígitos numéricos", "OK");
                return false;
            }

            if (!IsValidEmail(CorreoEntry.Text))
            {
                DisplayAlert("Error", "Por favor, ingrese un correo electrónico válido", "OK");
                return false;
            }

            if (ContraseñaEntry.Text != ConfirmarContraseñaEntry.Text)
            {
                DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase))
                return false;

            var domain = email.Split('@').Last();
            return validDomains.Contains(domain, StringComparer.OrdinalIgnoreCase);
        }
    }
}