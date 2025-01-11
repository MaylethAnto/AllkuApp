using AllkuApp.Dtos;
using AllkuApp.Servicios;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AllkuApp.Modelo;
using Xamarin.Essentials;
using System.Text.RegularExpressions;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        private readonly AuthService _authService;
        private const string MASTER_PASSWORD = "TuClaveSecretaUnica2024$";

        private readonly string[] validDomains = {
            "gmail.com", "outlook.com", "yahoo.com", "hotmail.com",
            "edu.ec", "edu.mx", "edu.ar", "edu.cl", "edu.pe", "edu.br", "edu.ve",
            "edu.bo", "edu.uy", "edu.cr", "edu.pa", "edu.ni", "edu.hn", "edu.sv",
            "edu.gt", "edu.do", "edu.pr"
        };

        public RegisterPage()
        {
            InitializeComponent();
            _authService = new AuthService();

            // Configuración inicial de visibilidad
            ClaveMaestraStack.IsVisible = false;
            DireccionCelularStack.IsVisible = false;

            // Configurar el Picker de roles
            RolPicker.SelectedIndexChanged += OnRoleChanged;
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navegar de regreso a la página anterior
            await Navigation.PopAsync();
        }
        private void OnRoleChanged(object sender, EventArgs e)
        {
            if (RolPicker.SelectedItem == null) return;

            var selectedRole = RolPicker.SelectedItem.ToString();

            // Actualizar visibilidad basada en el rol
            ClaveMaestraStack.IsVisible = selectedRole == "Administrador";
            DireccionCelularStack.IsVisible = selectedRole == "Dueño";
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarCamposBasicos()) return;

                var cedula = CedulaEntry.Text.Trim();
                var nombre = NombreEntry.Text.Trim();
                var usuario = UsuarioEntry.Text.Trim();
                var correo = CorreoEntry.Text.Trim();
                var contrasena = ContraseñaEntry.Text;
                var rolSeleccionado = RolPicker.SelectedItem?.ToString();

                Debug.WriteLine($"Intentando registrar usuario con rol: {rolSeleccionado}");
                Debug.WriteLine($"Correo completo: {correo}");

                if (rolSeleccionado == "Administrador")
                {
                    await RegistrarAdministrador(cedula, nombre, usuario, correo, contrasena);
                }
                else if (rolSeleccionado == "Dueño")
                {
                    await RegistrarUsuario(cedula, nombre, usuario, correo, contrasena, rolSeleccionado);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnRegisterButtonClicked: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
            }
        }

        private bool ValidarCamposBasicos()
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(CedulaEntry.Text) ||
                    string.IsNullOrWhiteSpace(NombreEntry.Text) ||
                    string.IsNullOrWhiteSpace(UsuarioEntry.Text) ||
                    string.IsNullOrWhiteSpace(CorreoEntry.Text) ||
                    string.IsNullOrWhiteSpace(ContraseñaEntry.Text) ||
                    string.IsNullOrWhiteSpace(ConfirmarContraseñaEntry.Text) ||
                    RolPicker.SelectedItem == null)
                {
                    DisplayAlert("Error", "Por favor, complete todos los campos requeridos", "OK");
                    return false;
                }

                // Validar cédula
                if (CedulaEntry.Text.Length != 10 || !CedulaEntry.Text.All(char.IsDigit))
                {
                    DisplayAlert("Error", "La cédula debe tener 10 dígitos numéricos", "OK");
                    return false;
                }

                // Validar correo electrónico
                if (!IsValidEmail(CorreoEntry.Text))
                {
                    DisplayAlert("Error", "Por favor, ingrese un correo electrónico válido", "OK");
                    return false;
                }

                // Validar contraseñas
                if (ContraseñaEntry.Text != ConfirmarContraseñaEntry.Text)
                {
                    DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                    return false;
                }

                Debug.WriteLine("Validación básica exitosa");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ValidarCamposBasicos: {ex.Message}");
                return false;
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Usar expresión regular para validar el formato general del correo electrónico
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase))
                    return false;

                // Validar que el dominio del correo esté en la lista de dominios válidos
                var domain = email.Split('@').Last();
                return validDomains.Contains(domain, StringComparer.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private async Task RegistrarAdministrador(string cedula, string nombre, string usuario, string correo, string contrasena)
        {
            try
            {
                Debug.WriteLine("Iniciando registro de administrador");

                // Validar la clave maestra
                var claveMaestra = ClaveMaestraEntry?.Text;

                if (string.IsNullOrWhiteSpace(claveMaestra))
                {
                    await DisplayAlert("Error", "La clave maestra es requerida", "OK");
                    return;
                }

                if (claveMaestra != MASTER_PASSWORD)
                {
                    await DisplayAlert("Error", "La clave maestra es incorrecta", "OK");
                    return;
                }

                var adminDto = new RegistrarAdministradorDto
                {
                    CedulaAdministrador = cedula,
                    NombreAdministrador = nombre,
                    UsuarioAdministrador = usuario,
                    CorreoAdministrador = correo,
                    ContrasenaAdministrador = contrasena,
                    ClaveMaestra = claveMaestra
                };

                Debug.WriteLine("Enviando datos al servicio de registro");
                var result = await _authService.RegistrarAdministradorAsync(adminDto);

                if (result)
                {
                    await DisplayAlert("Éxito", "¡Administrador registrado exitosamente!", "OK");
                    LimpiarFormulario();
                    // Redirigir a Login Page                                    
                    await Navigation.PushAsync(new LoginPage());

                }
                else
                {
                    await DisplayAlert("Error", "No se pudo registrar el administrador", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en RegistrarAdministrador: {ex.Message}");
                await DisplayAlert("Error", $"Error al registrar administrador: {ex.Message}", "OK");
            }
        }

        private async Task RegistrarUsuario(string cedula, string nombre, string usuario, string correo, string contrasena, string rol)
        {
            try
            {
                Debug.WriteLine("Iniciando registro de usuario normal");

                // Validar celular para roles no administrador
                if (string.IsNullOrWhiteSpace(CelularEntry.Text) ||
                    CelularEntry.Text.Length != 10 ||
                    !CelularEntry.Text.All(char.IsDigit))
                {
                    await DisplayAlert("Error", "El número celular debe tener 10 dígitos numéricos", "OK");
                    return;
                }

                var direccion = DireccionEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(direccion))
                {
                    await DisplayAlert("Error", "La dirección es obligatoria", "OK");
                    return;
                }

                var registerDto = new RegistrarUsuarioDto
                {
                    Cedula = cedula,
                    Nombre = nombre,
                    Usuario = usuario,
                    Correo = correo,
                    Contrasena = contrasena,
                    Rol = rol,
                    Direccion = direccion,
                    Celular = CelularEntry.Text.Trim()
                };

                Debug.WriteLine("Enviando datos al servicio de registro de usuario");
                var result = await _authService.RegistrarUsuarioAsync(registerDto);

                if (result)
                {
                    // Guardar la cédula en las preferencias
                    Preferences.Set("CedulaDueno", cedula);
                    Debug.WriteLine($"Cédula guardada en preferencias: {cedula}");
                    await DisplayAlert("Éxito", "¡Usuario registrado exitosamente!", "OK");
                    LimpiarFormulario();

                    // En lugar de navegar a LoginPage, establecerla como página principal
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo registrar el usuario", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en RegistrarUsuario: {ex.Message}");
                await DisplayAlert("Error", $"Error al registrar usuario: {ex.Message}", "OK");
            }
        }

        private void LimpiarFormulario()
        {
            try
            {
                CedulaEntry.Text = string.Empty;
                NombreEntry.Text = string.Empty;
                UsuarioEntry.Text = string.Empty;
                CorreoEntry.Text = string.Empty;
                ContraseñaEntry.Text = string.Empty;
                ConfirmarContraseñaEntry.Text = string.Empty;
                DireccionEntry.Text = string.Empty;
                CelularEntry.Text = string.Empty;
                ClaveMaestraEntry.Text = string.Empty;
                RolPicker.SelectedItem = null;

                // Restablecer visibilidad
                ClaveMaestraStack.IsVisible = false;
                DireccionCelularStack.IsVisible = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en LimpiarFormulario: {ex.Message}");
            }
        }
    }
}