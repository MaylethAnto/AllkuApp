using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AllkuApp.Modelo;
using AllkuApp.Servicios;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CrearHistorialPage : ContentPage
    {
        private readonly Historial_ClinicoService _historialService;
        private readonly int _idCanino;

        public CrearHistorialPage(int idCanino)
        {
            InitializeComponent();
            _historialService = new Historial_ClinicoService();
            _idCanino = idCanino;

            Debug.WriteLine($"CrearHistorialPage inicializada con idCanino: {_idCanino}");

            // Configurar valores iniciales
            FechaHistorial.Date = DateTime.Now;
            if (TipoHistorial.Items.Count > 0)
            {
                TipoHistorial.SelectedIndex = 0;
            }

            // Verificar el contador de aperturas
            if (Application.Current.Properties.ContainsKey("OpenCount"))
            {
                int openCount = (int)Application.Current.Properties["OpenCount"];
                if (openCount >= 2)
                {
                    // Si se ha abierto más de una vez, navegar a la página de listado
                    Navigation.PushAsync(new ListaHistorialesPage(_idCanino));
                }
                else
                {
                    // Incrementar el contador de aperturas
                    Application.Current.Properties["OpenCount"] = openCount + 1;
                }
            }
            else
            {
                // Primera apertura, inicializar el contador
                Application.Current.Properties["OpenCount"] = 1;
            }
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            if (!ValidarFormulario())
            {
                return;
            }

            try
            {
                IsBusy = true;

                var historialRequest = new AllkuApp.Servicios.HistorialRequest
                {
                    fecha_historial = FechaHistorial.Date,
                    tipo_historial = TipoHistorial.SelectedItem?.ToString(),
                    descripcion_historial = DescripcionHistorial.Text?.Trim(),
                    notificacion_historial = NotificacionHistorial.IsToggled,
                    id_canino = _idCanino
                };

                bool resultado = await _historialService.CreateHistorialClinicoAsync(historialRequest);

                if (resultado)
                {
                    await DisplayAlert("Éxito", "Historial clínico registrado correctamente", "OK");
                    await Navigation.PushAsync(new ListaHistorialesPage(_idCanino));
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo registrar el historial clínico", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidarFormulario()
        {
            if (TipoHistorial.SelectedItem == null)
            {
                DisplayAlert("Error", "Debe seleccionar un tipo de historial", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(DescripcionHistorial.Text))
            {
                DisplayAlert("Error", "Debe ingresar una descripción", "OK");
                return false;
            }

            if (_idCanino <= 0)
            {
                DisplayAlert("Error", "ID del canino no válido", "OK");
                return false;
            }

            return true;
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ListaHistorialesPage(_idCanino));

        }

    }

}