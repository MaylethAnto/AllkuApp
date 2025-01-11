using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Essentials;
using AllkuApp.Servicios;

namespace AllkuApp.Vista
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginService _loginService;
        private readonly AuthService _authService;

        public LoginPage()
        {
            InitializeComponent();
            _loginService = new LoginService();  // Para el inicio de sesión
            _authService = new AuthService();    // Para el registro de usuarios
        }

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
                Debug.WriteLine($"Response completo: {System.Text.Json.JsonSerializer.Serialize(response)}");

                if (response.Mensaje == "Login exitoso")
                {
                    var rol = response.Rol;
                    var cedulaDueno = response.CedulaDueno;
                    var celularPaseador = response.CelularPaseador;  // Usar la propiedad correcta
                    var nombrePaseador = response.NombrePaseador;
                    var esPrimeraVez = response.EsPrimeraVez;

                    // Agregar estas líneas de verificación
                    Debug.WriteLine($"Cédula recibida del servidor: {cedulaDueno}");
                    Debug.WriteLine($"Número de teléfono recibido del servidor: {(celularPaseador ?? "null")}");  // Verificar el número del paseador

                    if (!string.IsNullOrEmpty(cedulaDueno))
                    {
                        Preferences.Set("CedulaDueno", cedulaDueno);
                        // Verificar inmediatamente si se guardó
                        var verificacionCedula = Preferences.Get("CedulaDueno", "no encontrada");
                        Debug.WriteLine($"Verificación de cédula guardada: {verificacionCedula}");
                    }
                    else
                    {
                        Debug.WriteLine("Cédula vacía, no se guardará en las preferencias");
                    }

                    if (rol == "Paseador")
                    {
                        if (!string.IsNullOrEmpty(celularPaseador) && !string.IsNullOrEmpty(nombrePaseador))
                        {
                            Preferences.Set("CelularPaseador", celularPaseador);
                            Preferences.Set("NombrePaseador", nombrePaseador);

                            // Verificar inmediatamente
                            var verificacion = Preferences.Get("CelularPaseador", "no guardado");
                            var verificacionNombre = Preferences.Get("NombrePaseador", "no guardado");
                            Debug.WriteLine($"Número del paseador guardado: {verificacion}");
                            Debug.WriteLine($"Nombre del paseador guardado: {verificacionNombre}");
                        }
                        else
                        {
                            Debug.WriteLine("Número o el Nombre vacio, no se guardará en las preferencias");
                        }
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
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}