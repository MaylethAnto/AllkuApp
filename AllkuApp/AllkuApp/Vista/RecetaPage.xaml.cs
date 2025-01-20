using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using AllkuApp.Services;
using System.Linq;
using System.IO;

namespace AllkuApp.Vista
{
    public partial class RecetaPage : ContentPage
    {
        private readonly RecetaService _recetaService;
        private ObservableCollection<RecetaViewModel> _recetas;
        public ICommand RefreshCommand { get; private set; }

        public RecetaPage()
        {
            InitializeComponent();
            _recetaService = new RecetaService();
            _recetas = new ObservableCollection<RecetaViewModel>();
            RecetasListView.ItemsSource = _recetas;

            // Configurar el comando de actualización
            RefreshCommand = new Command(async () => await CargarRecetas());
            BindingContext = this;

            // Manejar la selección de recetas
            RecetasListView.ItemSelected += OnRecetaSelected;
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarRecetas();
        }

        private async Task CargarRecetas()
        {
            try
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                RefreshView.IsRefreshing = true;

                var recetas = await _recetaService.GetRecetasAsync();
                _recetas.Clear();

                foreach (var receta in recetas)
                {
                    _recetas.Add(new RecetaViewModel
                    {
                        id_receta = receta.id_receta,
                        nombre_receta = receta.nombre_receta,
                        descripcion_receta = receta.descripcion_receta,
                        foto_receta = receta.foto_receta,
                        id_canino = receta.id_canino,
                        HasImage = receta.foto_receta != null && receta.foto_receta.Length > 0,
                        ImageSource = receta.foto_receta != null && receta.foto_receta.Length > 0
                            ? ImageSource.FromStream(() => new MemoryStream(receta.foto_receta))
                            : null
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "No se pudieron cargar las recetas: " + ex.Message, "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                RefreshView.IsRefreshing = false;
            }
        }

        private async void OnRecetaSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            var recetaVM = e.SelectedItem as RecetaViewModel;
            RecetasListView.SelectedItem = null;

            try
            {
                var recetaCompleta = await _recetaService.GetRecetaByIdAsync(recetaVM.id_receta);
                // Aquí puedes navegar a la página de detalles

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "No se pudo cargar el detalle de la receta: " + ex.Message, "OK");
            }
        }
    }

    // ViewModel para manejar la presentación de las recetas
    public class RecetaViewModel : Receta
    {
        public bool HasImage { get; set; }
        public ImageSource ImageSource { get; set; }
    }
}