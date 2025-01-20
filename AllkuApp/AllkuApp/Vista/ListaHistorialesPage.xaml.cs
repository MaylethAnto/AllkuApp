using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AllkuApp.Servicios;
using AllkuApp.Vista;
using Xamarin.Forms;

namespace AllkuApp.Vista
{
    public partial class ListaHistorialesPage : ContentPage
    {
        private readonly Historial_ClinicoService _service;
        private readonly int _idCanino;
        public ObservableCollection<HistorialClinicoDto> Historiales { get; set; }
        public bool IsRefreshing { get; set; }

        public ListaHistorialesPage(int idCanino)
        {
            InitializeComponent();
            _service = new Historial_ClinicoService();
            _idCanino = idCanino;
            Historiales = new ObservableCollection<HistorialClinicoDto>();
            BindingContext = this;
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarHistoriales();
        }

        private async Task CargarHistoriales()
        {
            try
            {
                IsRefreshing = true;
                var historiales = await _service.GetHistorialesPorCaninoAsync(_idCanino);

                Historiales.Clear();
                foreach (var historial in historiales)
                {
                    Historiales.Add(historial);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar historiales: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async void OnNuevoHistorialClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CrearHistorialPage(_idCanino));
        }

    }

}