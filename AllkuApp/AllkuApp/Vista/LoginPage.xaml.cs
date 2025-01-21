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
            BindingContext = new LoginPageViewModel();
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
                    var cedulaPaseador = response.CedulaPaseador;
                    var celularPaseador = response.CelularPaseador;
                    var nombrePaseador = response.NombrePaseador;
                    var esPrimeraVez = response.EsPrimeraVez;

                    // Log de datos recibidos
                    Debug.WriteLine($"Rol recibido del servidor: {rol}");
                    Debug.WriteLine($"Cédula recibida del servidor: {cedulaDueno}");
                    Debug.WriteLine($"Cédula del paseador recibida del servidor: {cedulaPaseador}");
                    Debug.WriteLine($"Número de teléfono recibido del servidor: {celularPaseador ?? "null"}");
                    Debug.WriteLine($"Nombre del paseador recibido del servidor: {nombrePaseador ?? "null"}");

                    try
                    {
                        // Manejo de datos del dueño
                        if (!string.IsNullOrEmpty(cedulaDueno))
                        {
                            Preferences.Set("CedulaDueno", cedulaDueno);
                            var verificacionCedula = Preferences.Get("CedulaDueno", "no encontrada");
                            Debug.WriteLine($"Verificación de cédula del dueño guardada: {verificacionCedula}");
                        }

                        // Manejo específico para rol Paseador
                        if (rol == "Paseador")
                        {
                            // Guardar datos del paseador incluso si la cédula es null
                            if (!string.IsNullOrEmpty(celularPaseador) && !string.IsNullOrEmpty(nombrePaseador))
                            {
                                // Guardar todos los datos disponibles
                                if (!string.IsNullOrEmpty(cedulaPaseador))
                                {
                                    Preferences.Set("CedulaPaseador", cedulaPaseador);
                                }
                                Preferences.Set("CelularPaseador", celularPaseador);
                                Preferences.Set("NombrePaseador", nombrePaseador);

                                // Verificación de datos guardados
                                var verificacionCedula = Preferences.Get("CedulaPaseador", "no guardada");
                                var verificacionCelular = Preferences.Get("CelularPaseador", "no guardado");
                                var verificacionNombre = Preferences.Get("NombrePaseador", "no guardado");

                                Debug.WriteLine($"Verificación de datos guardados:");
                                Debug.WriteLine($"- Cédula del paseador: {verificacionCedula}");
                                Debug.WriteLine($"- Número del paseador: {verificacionCelular}");
                                Debug.WriteLine($"- Nombre del paseador: {verificacionNombre}");
                            }
                            else
                            {
                                Debug.WriteLine("Error: Datos esenciales del paseador faltantes (celular o nombre)");
                                throw new Exception("Datos incompletos del paseador");
                            }
                        }

                        // Guardar el rol del usuario
                        Preferences.Set("UserRole", rol);
                        Debug.WriteLine($"Rol guardado: {Preferences.Get("UserRole", "no guardado")}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error al guardar las preferencias: {ex.Message}");
                        // Aquí podrías mostrar un mensaje al usuario o manejar el error de otra manera
                        await Application.Current.MainPage.DisplayAlert("Error", "Hubo un problema al guardar los datos de sesión", "OK");
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
        private async void OnOlvidasteContrasenaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ForgotPasswordPage());
        }
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}