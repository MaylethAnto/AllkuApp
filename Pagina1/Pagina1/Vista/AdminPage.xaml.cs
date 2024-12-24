using Newtonsoft.Json;
using Pagina1.Modelo;
using Pagina1.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pagina1.Vista
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

        private async void OnRegistrarPaseadorClicked(object sender, EventArgs e)
        {
            // Navega a la página de registro de paseadores
            await Navigation.PushAsync(new RegistroPaseadorPage());
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
    }

}