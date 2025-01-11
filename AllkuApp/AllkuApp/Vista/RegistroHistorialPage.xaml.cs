using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AllkuApp.Servicios;
using AllkuApp.Dtos;
using AllkuApp.Modelo;
using AllkuApp.Modelo;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegistroHistorialPage : ContentPage
    {
        private readonly CaninoHistorialService _caninoHistorialService;
        public ObservableCollection<CaninoConHistorialDTO> Caninos { get; set; }
        public ObservableCollection<Historial_Clinico> Historiales { get; set; }

        public RegistroHistorialPage()
        {
            InitializeComponent();
            _caninoHistorialService = new CaninoHistorialService();
            Caninos = new ObservableCollection<CaninoConHistorialDTO>();
            Historiales = new ObservableCollection<Historial_Clinico>();
            BindingContext = this;

            // Verificar si es la primera vez que se abre la página
            if (Application.Current.Properties.ContainsKey("PrimeraVezRegistroHistorial") &&
                (bool)Application.Current.Properties["PrimeraVezRegistroHistorial"] == false)
            {
                MostrarListado();
            }
            else
            {
                MostrarFormulario();
                Application.Current.Properties["PrimeraVezRegistroHistorial"] = false;
                Application.Current.SavePropertiesAsync();
            }

            CargarCaninos();
            CargarHistoriales();
        }

        private void MostrarFormulario()
        {
            PageTitle.Text = "Registro de Historial Clínico";
            FormularioStackLayout.IsVisible = true;
            ListadoStackLayout.IsVisible = false;
        }

        private void MostrarListado()
        {
            PageTitle.Text = "Listado de Historiales Clínicos";
            FormularioStackLayout.IsVisible = false;
            ListadoStackLayout.IsVisible = true;
        }

        private async void CargarCaninos()
        {
            try
            {
                var listaCaninos = await _caninoHistorialService.ObtenerCaninosConHistorialAsync();
                if (listaCaninos != null)
                {
                    foreach (var canino in listaCaninos)
                    {
                        if (canino != null && !string.IsNullOrEmpty(canino.nombreCanino))
                        {
                            Caninos.Add(canino);
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No se encontraron caninos.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar los caninos: {ex.Message}", "OK");
            }
        }

        private async void CargarHistoriales()
        {
            try
            {
                // Lógica para cargar los historiales clínicos
                // Por ahora, se agregan historiales de ejemplo
                Historiales.Add(new Historial_Clinico { fecha_historial = DateTime.Now, tipo_historial = "Vacunación", descripcion_historial = "Vacuna contra la rabia" });
                Historiales.Add(new Historial_Clinico { fecha_historial = DateTime.Now.AddDays(-30), tipo_historial = "Desparasitación", descripcion_historial = "Desparasitación anual" });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar los historiales: {ex.Message}", "OK");
            }
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            if (ValidarCampos())
            {
                var caninoSeleccionado = CaninoPicker.SelectedItem as CaninoConHistorialDTO;
                if (caninoSeleccionado == null)
                {
                    await DisplayAlert("Error", "Por favor seleccione una mascota.", "OK");
                    return;
                }

                var nuevoHistorial = new HistorialRequest
                {
                    fecha_historial = FechaPicker.Date,
                    tipo_historial = TipoHistorialEntry.Text?.Trim(),
                    descripcion_historial = DescripcionEditor.Text?.Trim(),
                    notificacion_historial = NotificacionSwitch.IsToggled,
                    id_canino = caninoSeleccionado.idCanino
                };

                try
                {
                    var resultado = await _caninoHistorialService.CrearHistorialClinicoAsync(nuevoHistorial);

                    if (resultado)
                    {
                        // Convertir HistorialRequest a Historial_Clinico
                        var historialClinico = new Historial_Clinico
                        {
                            fecha_historial = nuevoHistorial.fecha_historial,
                            tipo_historial = nuevoHistorial.tipo_historial,
                            descripcion_historial = nuevoHistorial.descripcion_historial,
                            notificacion_historial = nuevoHistorial.notificacion_historial,
                            id_canino = nuevoHistorial.id_canino
                        };

                        await DisplayAlert("Éxito", "Historial clínico registrado exitosamente.", "OK");
                        LimpiarFormulario();
                        Historiales.Add(historialClinico);
                        MostrarListado();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Hubo un problema al registrar el historial clínico.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Hubo un problema al realizar la operación: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Por favor complete todos los campos.", "OK");
            }
        }

        private void OnCancelarClicked(object sender, EventArgs e)
        {
            LimpiarFormulario();
            MostrarListado();
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            LimpiarFormulario();
            MostrarFormulario();
        }

        private bool ValidarCampos()
        {
            return !string.IsNullOrWhiteSpace(TipoHistorialEntry.Text) &&
                   !string.IsNullOrWhiteSpace(DescripcionEditor.Text);
        }

        private void LimpiarFormulario()
        {
            TipoHistorialEntry.Text = string.Empty;
            DescripcionEditor.Text = string.Empty;
            FechaPicker.Date = DateTime.Now;
            NotificacionSwitch.IsToggled = false;
            CaninoPicker.SelectedIndex = -1;
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}