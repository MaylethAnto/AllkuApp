using AllkuApp.Vista;
using Newtonsoft.Json;
using AllkuApp.Modelo;
using AllkuApp.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdminPage : ContentPage
    {
        private readonly AdminService _adminService;

        public AdminPage()
        {
            InitializeComponent();
            _adminService = new AdminService(this);
            CargarDatosIniciales();

            // Configurar el Picker
            SeleccionTipoUsuario.Items.Add("Dueños");
            SeleccionTipoUsuario.Items.Add("Caninos");
            SeleccionTipoUsuario.Items.Add("Paseadores");
            SeleccionTipoUsuario.SelectedIndex = 0; // Seleccionar Dueños por defecto

            SeleccionTipoUsuario.SelectedIndexChanged += OnTipoUsuarioSeleccionado;
        }

      
        private async void CargarDatosIniciales()
        {
            try
            {
                // Llamamos al servicio para obtener los datos de los paseadores, dueños y caninos
                var paseadores = await _adminService.ObtenerDatos<Paseador>("paseadores");
                var caninos = await _adminService.ObtenerDatos<Canino>("caninos");
                var duenos = await _adminService.ObtenerDatos<Dueno>("dueños");

                // Asignamos los datos deserializados a los ListViews
                ListaPaseadores.ItemsSource = paseadores ?? new List<Paseador>();
                ListaCaninos.ItemsSource = caninos ?? new List<Canino>();
                ListaDuenos.ItemsSource = duenos ?? new List<Dueno>();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudieron cargar los datos: {ex.Message}", "Aceptar");
            }
        }

        private async void OnPaseadorSwitchToggled(object sender, ToggledEventArgs e)
        {
            var switchControl = (Switch)sender;
            var paseador = switchControl.BindingContext as Paseador;

            if (paseador != null)
            {
                bool resultado = await _adminService.TogglePaseadorEstado(paseador.CedulaPaseador.ToString());

                if (!resultado)
                {
                    // Revert the switch if API call fails
                    switchControl.IsToggled = !switchControl.IsToggled;
                }
            }
        }

        // Evento para manejar la selección de tipo de usuario
        private void OnTipoUsuarioSeleccionado(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            switch (picker.SelectedIndex)
            {
                case 0: // Dueños
                    StackCaninos.IsVisible = false;
                    StackDuenos.IsVisible = true;
                    StackPaseadores.IsVisible = false;
                    break;
                case 1: // Caninos
                    StackCaninos.IsVisible = true;
                    StackDuenos.IsVisible = false;
                    StackPaseadores.IsVisible = false;
                    break;
                case 2: // Paseadores
                    StackCaninos.IsVisible = false;
                    StackDuenos.IsVisible = false;
                    StackPaseadores.IsVisible = true;
                    break;
            }
        }

        private async void ImageButton_Clicked(object sender, EventArgs e)
        {
            // Navegar a la página de menú
           await Navigation.PushAsync(new MenuAdminPage());
        }

        private async void OnRecetasButtonClickedd(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RecetaFormulario());


        }
    }

}