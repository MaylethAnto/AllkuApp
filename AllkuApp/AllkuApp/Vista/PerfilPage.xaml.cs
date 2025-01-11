using AllkuApp.Dtos;
using AllkuApp.Modelo;
using AllkuApp.Servicios;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AllkuApp.Vista
{
    public partial class PerfilPage : ContentPage
    {
        private PerfilCanino perfilCanino;
        private readonly ApiService _apiService;

        public PerfilPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            CargarDatosPerfil();
        }

        private async void CargarDatosPerfil()
        {
            try
            {
                // Obtener la cédula del dueño desde Preferences
                var cedulaDueno = Preferences.Get("CedulaDueno", string.Empty);
                if (string.IsNullOrEmpty(cedulaDueno))
                {
                    await DisplayAlert("Error", "No se pudo obtener la cédula del dueño.", "OK");
                    return;
                }

                // Consumir el API para obtener los datos del canino
                var caninos = await _apiService.GetCaninosByCedulaDuenoAsync(cedulaDueno);
                if (caninos != null && caninos.Count > 0)
                {
                    // Asignar los datos a los controles
                    var caninoDto = caninos[0];
                    perfilCanino = new PerfilCanino
                    {
                        IdCanino = caninoDto.IdCanino,
                        NombreCanino = caninoDto.NombreCanino,
                        RazaCanino = caninoDto.RazaCanino,
                        EdadCanino = caninoDto.EdadCanino,
                        PesoCanino = (decimal)caninoDto.PesoCanino,
                        FotoCanino = caninoDto.FotoCanino,
                        CelularDueno = caninoDto.CelularDueno
                    };

                    // Actualizar la UI con los datos
                    if (perfilCanino.EdadCanino > 0)
                    {
                        EdadCaninoPicker.SelectedItem = perfilCanino.EdadCanino;
                    }

                    if (perfilCanino.PesoCanino > 0)
                    {
                        PesoCaninoEntry.Text = perfilCanino.PesoCanino.ToString();
                    }

                    if (!string.IsNullOrEmpty(perfilCanino.CelularDueno))
                    {
                        CelularDuenoEntry.Text = perfilCanino.CelularDueno;
                    }

                    if (perfilCanino.FotoCanino != null && perfilCanino.FotoCanino.Length > 0)
                    {
                        FotoCanino.Source = ImageSource.FromStream(() => new MemoryStream(perfilCanino.FotoCanino));
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No se encontraron datos del canino.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "No se pudieron cargar los datos del perfil: " + ex.Message, "OK");
            }
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnCambiarFotoClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Selecciona una foto"
                });

                if (result != null)
                {
                    using (var stream = await result.OpenReadAsync())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            perfilCanino.FotoCanino = memoryStream.ToArray();
                        }

                        FotoCanino.Source = ImageSource.FromStream(() => new MemoryStream(perfilCanino.FotoCanino));
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo cambiar la foto: {ex.Message}", "OK");
            }
        }

        private async void OnGuardarCambiosClicked(object sender, EventArgs e)
        {
            try
            {
                // Validar los datos ingresados
                if (string.IsNullOrWhiteSpace(CelularDuenoEntry.Text) || CelularDuenoEntry.Text.Length != 10)
                {
                    await DisplayAlert("Error", "El número de celular debe tener 10 dígitos.", "OK");
                    return;
                }

                if (EdadCaninoPicker.SelectedItem == null)
                {
                    await DisplayAlert("Error", "Por favor, selecciona una edad para el canino.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(PesoCaninoEntry.Text) || !decimal.TryParse(PesoCaninoEntry.Text, out decimal pesoCanino))
                {
                    await DisplayAlert("Error", "Por favor, ingresa un peso válido para el canino.", "OK");
                    return;
                }

                // Actualizar solo los campos permitidos
                var actualizacionPerfil = new
                {
                    IdCanino = perfilCanino.IdCanino,
                    EdadCanino = (int)EdadCaninoPicker.SelectedItem,
                    PesoCanino = decimal.Parse(PesoCaninoEntry.Text),
                    FotoCanino = perfilCanino.FotoCanino,
                    CelularDueno = CelularDuenoEntry.Text
                };

                // Guardar en Preferences solo los campos actualizables
                Preferences.Set("EdadCanino", actualizacionPerfil.EdadCanino);
                Preferences.Set("PesoCanino", (double)actualizacionPerfil.PesoCanino);
                Preferences.Set("FotoCanino", actualizacionPerfil.FotoCanino != null ? Convert.ToBase64String(actualizacionPerfil.FotoCanino) : string.Empty);

                // Notificar al MainPage sobre la actualización
                MessagingCenter.Send(this, "PerfilActualizado", actualizacionPerfil);

                await DisplayAlert("Éxito", "Perfil actualizado correctamente", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "No se pudieron guardar los cambios: " + ex.Message, "OK");
            }
        }
    }
}