using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AllkuApp.Modelo;
using AllkuApp.Servicios;
using Plugin.Media;
using Plugin.Media.Abstractions;
using AllkuApp.Services;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecetaFormulario : ContentPage
    {
        private readonly RecetaService _recetaService;
        private string _fotoPath;

        public RecetaFormulario()
        {
            InitializeComponent();
            _recetaService = new RecetaService();
        }

        /// <summary>
        /// Maneja el evento de clic para seleccionar una foto.
        /// </summary>
        private async void OnSeleccionarFotoClicked(object sender, EventArgs e)
        {
            if (!CrossMedia.IsSupported || !CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Error", "Seleccionar foto no está soportado en este dispositivo.", "OK");
                return;
            }

            var photo = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
            });

            if (photo != null)
            {
                _fotoPath = photo.Path;
                FotoRecetaImage.Source = ImageSource.FromFile(_fotoPath);

                using (var stream = photo.GetStream())
                {
                    using (var memoryStream = new System.IO.MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        var imageBytes = memoryStream.ToArray();
                        FotoRecetaEntry.Text = Convert.ToBase64String(imageBytes);
                    }
                }
            }
        }

        /// <summary>
        /// Maneja el evento de clic para guardar una receta.
        /// </summary>
        private async void OnGuardarRecetaClicked(object sender, EventArgs e)
        {
            if (ValidarCampos())
            {
                var nuevaReceta = new CreateRecetaRequest
                {
                    nombre_receta = NombreRecetaEntry.Text,
                    descripcion_receta = DescripcionRecetaEditor.Text,
                    foto_receta = Convert.FromBase64String(FotoRecetaEntry.Text),
                    id_canino = int.Parse(IdCaninoEntry.Text)
                };

                var result = await _recetaService.CreateRecetaAsync(nuevaReceta);

                if (result)
                {
                    MessagingCenter.Send(this, "RecetaCreada", nuevaReceta);
                    await DisplayAlert("Éxito", "Receta registrada correctamente.", "OK");
                    LimpiarFormulario();
                }
                else
                {
                    await DisplayAlert("Error", "Ocurrió un error al guardar la receta.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Por favor, complete todos los campos.", "OK");
            }
        }

        private void OnCancelarClicked(object sender, EventArgs e)
        {
            // Lógica para manejar el evento de clic en el botón "Cancelar"
            // Por ejemplo, limpiar el formulario o navegar a otra página
            LimpiarFormulario(); // Método para limpiar el formulario
            DisplayAlert("Cancelado", "Operación cancelada", "OK");
        }

        /// <summary>
        /// Valida que todos los campos necesarios estén completos.
        /// </summary>
        /// <returns>True si todos los campos son válidos, de lo contrario False.</returns>
        private bool ValidarCampos()
        {
            return !string.IsNullOrWhiteSpace(NombreRecetaEntry.Text) &&
                   !string.IsNullOrWhiteSpace(DescripcionRecetaEditor.Text) &&
                   !string.IsNullOrWhiteSpace(FotoRecetaEntry.Text) &&
                   !string.IsNullOrWhiteSpace(IdCaninoEntry.Text);
        }

        /// <summary>
        /// Limpia el formulario después de guardar la receta.
        /// </summary>
        private void LimpiarFormulario()
        {
            NombreRecetaEntry.Text = string.Empty;
            DescripcionRecetaEditor.Text = string.Empty;
            FotoRecetaEntry.Text = string.Empty;
            FotoRecetaImage.Source = null;
            IdCaninoEntry.Text = string.Empty;
        }

        /// <summary>
        /// Maneja el evento de clic para volver a la página anterior.
        /// </summary>
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}