using AllkuApp.Dtos;
using AllkuApp.Servicios;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;

namespace AllkuApp.Vista
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

        private readonly ValidationService _validationService;

        public RegistroPaseadorPage()
        {
            InitializeComponent();
            _authService = new AuthService();
            _validationService = new ValidationService();
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navegar de regreso a la página anterior
            await Navigation.PushAsync(new MenuAdminPage());
        }
        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (!await ValidarCampos()) return;

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

        private async Task<bool> ValidarCampos()
        {
            try
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
                    await DisplayAlert("Error", "Por favor, complete todos los campos requeridos", "OK");
                    return false;
                }

                // Validar formato de cédula
                if (CedulaEntry.Text.Length != 10 || !CedulaEntry.Text.All(char.IsDigit))
                {
                    await DisplayAlert("Error", "La cédula debe tener 10 dígitos numéricos", "OK");
                    return false;
                }

                // Validar si la cédula ya existe
                if (await _validationService.VerificarCedulaAsync(CedulaEntry.Text))
                {
                    await DisplayAlert("Error", "Esta cédula ya está registrada en el sistema", "OK");
                    return false;
                }

                // Validar si el usuario ya existe
                if (await _validationService.VerificarUsuarioAsync(UsuarioEntry.Text))
                {
                    await DisplayAlert("Error", "Este nombre de usuario ya está en uso", "OK");
                    return false;
                }

                // Validar correo electrónico
                if (!IsValidEmail(CorreoEntry.Text))
                {
                    await DisplayAlert("Error", "Por favor, ingrese un correo electrónico válido", "OK");
                    return false;
                }

                // Validar si el correo ya existe
                if (await _validationService.VerificarCorreoAsync(CorreoEntry.Text))
                {
                    await DisplayAlert("Error", "Este correo electrónico ya está registrado", "OK");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ValidarCampos: {ex.Message}");
                await DisplayAlert("Error", "Error al validar los campos. Por favor, intente nuevamente.", "OK");
                return false;
            }
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