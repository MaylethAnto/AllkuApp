using AllkuApp.Modelo;
using AllkuApp.Servicios;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegistroMascotaPage : ContentPage
    {
        private readonly ApiService _apiService;
        private string _fotoPath;
        private const long MAX_IMAGE_SIZE = 5 * 1024 * 1024; // 5 MB

        public RegistroMascotaPage()
        {
            InitializeComponent();
            _apiService = new ApiService();

            // Generar una lista de edades de 1 a 25 años
            var edades = Enumerable.Range(1, 25).ToList();
            EdadPicker.ItemsSource = edades;

            // Agregar esta verificación en el constructor
            var cedula = Preferences.Get("CedulaDueno", string.Empty);
            Debug.WriteLine($"Cédula recuperada en constructor de RegistroMascotaPage: {cedula}");

        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navegar de regreso a la página anterior
            await Navigation.PopAsync();
        }

        // Evento cuando se selecciona una edad del Picker
        private void OnEdadPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (EdadPicker.SelectedIndex != -1)
            {
                // Obtener la edad seleccionada
                int edadSeleccionada = (int)EdadPicker.SelectedItem;
                // Realiza lo que necesites con la edad seleccionada, por ejemplo:
                DisplayAlert("Edad Seleccionada", $"Has seleccionado {edadSeleccionada} años.", "OK");
            }
        }

        // Seleccionar foto
        private async void OnSeleccionarFotoClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Selecciona una foto"
                });

                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    if (stream.Length > MAX_IMAGE_SIZE)
                    {
                        await DisplayAlert("Error", "El tamaño de la foto no debe exceder los 5 MB", "OK");
                        return;
                    }

                    _fotoPath = result.FullPath;
                    FotoImagen.Source = ImageSource.FromStream(() => stream);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo seleccionar la foto: {ex.Message}", "OK");
            }
        }

        // Guardar mascota
        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            if (!ValidarCampos())
                return;

            try
            {
                string cedula = Preferences.Get("CedulaDueno", string.Empty);
                Debug.WriteLine($"Cédula al momento de registrar mascota: {cedula}");

                if (string.IsNullOrEmpty(cedula))
                {
                    await DisplayAlert("Error", "No se pudo recuperar la cédula del usuario. Por favor, inicie sesión nuevamente.", "OK");
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                    return;
                }

                var nuevaMascota = new CaninoRequest
                {
                    NombreCanino = NombreEntry.Text.Trim(),
                    EdadCanino = (int)EdadPicker.SelectedItem,
                    RazaCanino = RazaPicker.SelectedItem.ToString(),
                    PesoCanino = decimal.Parse(PesoEntry.Text),
                    FotoCanino = !string.IsNullOrEmpty(_fotoPath) ? File.ReadAllBytes(_fotoPath) : null,
                    CedulaDueno = cedula
                };

                var resultado = await _apiService.RegistrarCaninoAsync(nuevaMascota);

                if (resultado == "Success")
                {
                    await DisplayAlert("Éxito", "Mascota registrada correctamente.", "OK");
                    LimpiarFormulario();

                    // Establecer MainPage como página principal
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo registrar la mascota.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en registro de mascota: {ex.Message}");
                await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
            }
        }
        // Validación de campos
        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(NombreEntry.Text) ||
                EdadPicker.SelectedIndex == -1 ||
                RazaPicker.SelectedItem == null ||
                string.IsNullOrWhiteSpace(PesoEntry.Text))
            {
                DisplayAlert("Error", "Por favor completa todos los campos.", "OK");
                return false;
            }

            if (!decimal.TryParse(PesoEntry.Text, out _))
            {
                DisplayAlert("Error", "Peso debe ser un valor numérico.", "OK");
                return false;
            }

            if (FotoImagen.Source == null)
            {
                DisplayAlert("Error", "Por favor selecciona una foto de perfil.", "OK");
                return false;
            }

            return true;
        }

        // Limpiar formulario
        private void LimpiarFormulario()
        {
            NombreEntry.Text = string.Empty;
            EdadPicker.SelectedIndex = -1;
            RazaPicker.SelectedItem = null;
            PesoEntry.Text = string.Empty;
            FotoImagen.Source = null;
            _fotoPath = null;
        }

        // Cancelar
        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            // Mostrar una alerta de confirmación
            var respuesta = await DisplayAlert(
                "Confirmación",
                "¿Deseas salir de la sesión?",
                "Sí",
                "No"
            );

            if (respuesta)
            {
                // Lógica para salir de la sesión
                await DisplayAlert("Sesión", "Has salido de la sesión", "OK");
                // Aquí puedes agregar código para cerrar la sesión o navegar a una página de inicio de sesión
                // Por ejemplo, si tienes una página de inicio de sesión llamada LoginPage:
                App.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }
    }
}