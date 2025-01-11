using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using AllkuApp.Servicios;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EjerciciosPage : ContentPage
    {
        private Label _mensajeNoRecorridos;
        public ObservableCollection<DistanciaRecorrida> Distancias { get; set; }
        private readonly ApiService _apiService;

        public EjerciciosPage()
        {
            InitializeComponent();
            Distancias = new ObservableCollection<DistanciaRecorrida>();
            DistanciaListView.ItemsSource = Distancias;
            _apiService = new ApiService();

            // Crear el mensaje de no recorridos
            _mensajeNoRecorridos = new Label
            {
                Text = "Aún no hay recorridos registrados. ¡Registra tu primer recorrido!",
                TextColor = Color.FromHex("#666666"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20, 0, 0),
                IsVisible = false
            };

            // Encontrar el StackLayout correcto dentro de la jerarquía
            if (Content is Grid mainGrid &&
                mainGrid.Children[1] is ScrollView scrollView &&
                scrollView.Content is StackLayout stackLayout)
            {
                stackLayout.Children.Add(_mensajeNoRecorridos);
            }
            else
            {
                Debug.WriteLine("No se pudo encontrar el StackLayout para agregar el mensaje");
            }

            // Cargar el historial al iniciar la página
            _ = CargarHistorialRecorridos();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = CargarHistorialRecorridos(); // Usamos _ = para evitar warning de Task no esperado
        }

        private async Task CargarHistorialRecorridos()
        {
            try
            {
                int idCanino = Preferences.Get("CaninoId", -1);
                if (idCanino != -1)
                {
                    DateTime fechaHoraInicio = FechaInicio.Date.Add(HoraInicio.Time);
                    DateTime fechaHoraFin = FechaFin.Date.Add(HoraFin.Time);

                    // Suponiendo que devuelve un único HistorialRecorrido
                    var historial = await _apiService.ObtenerHistorialRecorridosAsync();

                    if (historial != null)
                    {
                        DistanciaListView.ItemsSource = new List<DistanciaRecorrida> { historial };
                    }
                    else
                    {
                        await DisplayAlert("Información", "No hay recorridos registrados.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar historial: {ex.Message}");
                await DisplayAlert("Error", "No se pudo cargar el historial de recorridos.", "OK");
            }
        }





        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnObtenerDistanciaClicked(object sender, EventArgs e)
        {
            // Validar fechas
            if (FechaInicio.Date == FechaFin.Date && HoraInicio.Time >= HoraFin.Time)
            {
                await DisplayAlert("Error", "La hora de inicio debe ser anterior a la hora de fin en el mismo día.", "OK");
                return;
            }
            else if (FechaInicio.Date > FechaFin.Date)
            {
                await DisplayAlert("Error", "La fecha de inicio no puede ser posterior a la fecha de fin.", "OK");
                return;
            }

            // Combinar fecha y hora
            DateTime fechaHoraInicio = FechaInicio.Date.Add(HoraInicio.Time);
            DateTime fechaHoraFin = FechaFin.Date.Add(HoraFin.Time);

            try
            {
                int idCanino = Preferences.Get("CaninoId", -1);
                if (idCanino == -1)
                {
                    await DisplayAlert("Error", "No se encontró el ID del canino. Por favor, inicie sesión nuevamente.", "OK");
                    return;
                }

                // Mostrar indicador de carga
                IsBusy = true;

                var distancia = await _apiService.ObtenerDistanciaRecorridaAsync(idCanino, fechaHoraInicio, fechaHoraFin);

                if (distancia != null)
                {
                    // Después de obtener el nuevo recorrido, recargar todo el historial
                    await CargarHistorialRecorridos();
                    await DisplayAlert("Éxito", "Recorrido registrado correctamente", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener la distancia recorrida: {ex.Message}");

                string mensajeError = "No se pudo obtener la distancia recorrida";
                if (ex is HttpRequestException)
                {
                    mensajeError += ". Por favor, verifica tu conexión a internet.";
                }
                else if (ex is JsonException)
                {
                    mensajeError += ". Error al procesar los datos del servidor.";
                }

                await DisplayAlert("Error", mensajeError, "OK");
            }
            finally
            {
                // Ocultar indicador de carga
                IsBusy = false;
            }
        }

    }
}