using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;
using Pagina1.Vista;
using Pagina1.Servicios;
using System.Diagnostics;

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
                    var cedulaDueno = response.CedulaDueno;
                    var esPrimeraVez = response.EsPrimeraVez;

                    // Agregar estas líneas de verificación
                    Debug.WriteLine($"Cédula recibida del servidor: {cedulaDueno}");

                    if (!string.IsNullOrEmpty(cedulaDueno))
                    {
                        Preferences.Set("CedulaDueno", cedulaDueno);
                        // Verificar inmediatamente si se guardó
                        var verificacion = Preferences.Get("CedulaDueno", "no encontrada");
                        Debug.WriteLine($"Verificación de cédula guardada: {verificacion}");
                    }
                    else
                    {
                        Debug.WriteLine("Cédula vacía, no se guardará en las preferencias");
                    }

                    // Crear la página de destino según el rol
                    Page destinationPage = null;
                    switch (rol)
                    {
                        case "Administrador":
                            destinationPage = new AdminPage();
                            break;

                        case "Dueño":
                            if (esPrimeraVez)
                            {
                                destinationPage = new RegistroMascotaPage();
                            }
                            else
                            {
                                destinationPage = new MainPage();
                            }
                            break;

                        case "Paseador":
                            destinationPage = new PaseadoresPage();
                            break;

                        default:
                            statusLabel.TextColor = Color.Red;
                            statusLabel.Text = "Rol desconocido, contacte al administrador.";
                            return;
                    }

                    // Establecer la nueva página principal
                    Application.Current.MainPage = new NavigationPage(destinationPage);
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

        // Método para verificar si es la primera vez que inicia sesión
        private async Task<bool> VerificarPrimeraVez(string nombreUsuario)
        {
            try
            {
                var esPrimeraVez = await _loginService.EsPrimeraVez(nombreUsuario);
                return esPrimeraVez;
            }
            catch (Exception ex)
            {
                statusLabel.TextColor = Color.Red;
                statusLabel.Text = "Error al verificar si es la primera vez: " + ex.Message;
                return false;
            }
        }
    }
}