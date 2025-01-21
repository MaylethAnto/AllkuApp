using AllkuApp.Dtos;
using AllkuApp.Modelo;
using AllkuApp.Servicios;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AllkuApp.Vista
{
    public partial class PaseadoresPage : ContentPage
    {
        private readonly ApiService _apiService;
        private ObservableCollection<SolicitudPaseoResponseDto> _solicitudes;


        public PaseadoresPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _solicitudes = new ObservableCollection<SolicitudPaseoResponseDto>();
            SolicitudesListView.ItemsSource = _solicitudes;
            CargarSolicitudesGuardadas();
        }

        private void CargarSolicitudesGuardadas()
        {
            var solicitudesGuardadas = Preferences.Get("SolicitudesPaseador", string.Empty);
            if (!string.IsNullOrEmpty(solicitudesGuardadas))
            {
                try
                {
                    var solicitudesDeserializadas = JsonConvert.DeserializeObject<ObservableCollection<SolicitudPaseoResponseDto>>(solicitudesGuardadas);
                    _solicitudes.Clear();

                    foreach (var solicitud in solicitudesDeserializadas)
                    {
                        ActualizarEstadoBotones(solicitud);
                        _solicitudes.Add(solicitud);
                    }

                    // Ordenar las solicitudes después de cargarlas
                    var solicitudesOrdenadas = new ObservableCollection<SolicitudPaseoResponseDto>(
                        _solicitudes.OrderByDescending(s => s.FechaSolicitud)
                    );
                    _solicitudes = solicitudesOrdenadas;
                    SolicitudesListView.ItemsSource = _solicitudes;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al deserializar solicitudes: {ex.Message}");
                    _solicitudes = new ObservableCollection<SolicitudPaseoResponseDto>();
                }
            }
            else
            {
                _solicitudes = new ObservableCollection<SolicitudPaseoResponseDto>();
            }

            SolicitudesListView.ItemsSource = _solicitudes;
        }
        private void ActualizarEstadoBotones(SolicitudPaseoResponseDto solicitud)
        {
            // Normalizar el estado a minúsculas para comparación consistente
            var estado = solicitud.Estado?.ToLower();

            switch (estado)
            {
                case "pendiente":
                    solicitud.MostrarBotonesRespuesta = true;
                    solicitud.MostrarBotonesPaseo = false;
                    solicitud.PuedeIniciar = false;
                    solicitud.PuedeFinalizar = false;
                    break;
                case "aceptada":
                    solicitud.MostrarBotonesRespuesta = false;
                    solicitud.MostrarBotonesPaseo = true;
                    solicitud.PuedeIniciar = true;
                    solicitud.PuedeFinalizar = false;
                    break;
                case "en progreso":
                case "en curso":
                    solicitud.MostrarBotonesRespuesta = false;
                    solicitud.MostrarBotonesPaseo = true;
                    solicitud.PuedeIniciar = false;
                    solicitud.PuedeFinalizar = true;
                    break;
                case "finalizado":
                case "Finalizado": // Agregar ambas variantes
                    solicitud.MostrarBotonesRespuesta = false;
                    solicitud.MostrarBotonesPaseo = false;
                    solicitud.PuedeIniciar = false;
                    solicitud.PuedeFinalizar = false;
                    break;
                default:
                    solicitud.MostrarBotonesRespuesta = false;
                    solicitud.MostrarBotonesPaseo = false;
                    solicitud.PuedeIniciar = false;
                    solicitud.PuedeFinalizar = false;
                    break;
            }
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarSolicitudes();
        }

        private async Task CargarSolicitudes()
        {
            try
            {
                string cedula = Preferences.Get("CedulaPaseador", string.Empty);
                if (string.IsNullOrEmpty(cedula))
                {
                    await DisplayAlert("Error", "No se encontró la cédula del paseador.", "OK");
                    return;
                }

                var nuevasSolicitudes = await _apiService.GetSolicitudesPaseadorAsync(cedula);

                // Ordenar las nuevas solicitudes por fecha
                nuevasSolicitudes = nuevasSolicitudes.OrderByDescending(s => s.FechaSolicitud).ToList();

                foreach (var nuevaSolicitud in nuevasSolicitudes)
                {
                    // Verificar si la solicitud ya existe
                    var solicitudExistente = _solicitudes.FirstOrDefault(s => s.IdSolicitud == nuevaSolicitud.IdSolicitud);

                    if (solicitudExistente == null)
                    {
                        // Si es una nueva solicitud, configurar sus propiedades
                        nuevaSolicitud.MostrarBotonesRespuesta = true;
                        nuevaSolicitud.MostrarBotonesPaseo = false;
                        nuevaSolicitud.PuedeIniciar = false;
                        nuevaSolicitud.PuedeFinalizar = false;

                        // Convertir fechas a hora de Ecuador (UTC-5)
                        nuevaSolicitud.FechaSolicitud = TimeZoneInfo.ConvertTimeFromUtc(
                            nuevaSolicitud.FechaSolicitud,
                            TimeZoneInfo.CreateCustomTimeZone(
                                "Ecuador Time",
                                new TimeSpan(-5, 0, 0),
                                "Ecuador Time",
                                "Ecuador Time"
                            )
                        );

                        _solicitudes.Add(nuevaSolicitud);
                    }
                    else
                    {
                        // Si la solicitud ya existe, actualizar su estado si es necesario
                        ActualizarEstadoBotones(solicitudExistente);
                    }
                }

                // Ordenar la colección completa
                var solicitudesOrdenadas = new ObservableCollection<SolicitudPaseoResponseDto>(
                    _solicitudes.OrderByDescending(s => s.FechaSolicitud)
                );
                _solicitudes = solicitudesOrdenadas;
                SolicitudesListView.ItemsSource = _solicitudes;

                // Guardar el estado actual en las preferencias
                GuardarSolicitudes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar solicitudes: {ex.Message}");
                await DisplayAlert("Error", "No se pudieron cargar las solicitudes.", "OK");
            }
        }
        private void GuardarSolicitudes()
        {
            try
            {
                var solicitudesJson = JsonConvert.SerializeObject(_solicitudes);
                Preferences.Set("SolicitudesPaseador", solicitudesJson);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al guardar solicitudes: {ex.Message}");
            }
        }

        private async void OnEliminarClicked(object sender, EventArgs e)
        {
            try
            {
                var solicitud = (SolicitudPaseoResponseDto)((Button)sender).CommandParameter;

                bool confirmar = await DisplayAlert("Confirmar",
                    "¿Estás seguro de que deseas eliminar esta solicitud?",
                    "Sí", "No");

                if (confirmar)
                {
                    _solicitudes.Remove(solicitud);
                    GuardarSolicitudes();
                    await DisplayAlert("Éxito", "Solicitud eliminada correctamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al eliminar solicitud: {ex.Message}");
                await DisplayAlert("Error", "No se pudo eliminar la solicitud.", "OK");
            }
        }


        private async void OnAceptarClicked(object sender, EventArgs e)
        {
            var solicitud = (SolicitudPaseoResponseDto)((Button)sender).CommandParameter;
            await ResponderSolicitud(solicitud, true);
            GuardarSolicitudes();
        }

        private async void OnRechazarClicked(object sender, EventArgs e)
        {
            var solicitud = (SolicitudPaseoResponseDto)((Button)sender).CommandParameter;
            await ResponderSolicitud(solicitud, false);
            GuardarSolicitudes();
        }

        private async void OnIniciarPaseoClicked(object sender, EventArgs e)
        {
            var solicitud = (SolicitudPaseoResponseDto)((Button)sender).CommandParameter;
            solicitud.Estado = "En Progreso";
            solicitud.PuedeIniciar = false;
            solicitud.PuedeFinalizar = true;
            solicitud.MostrarBotonesPaseo = true;
            GuardarSolicitudes();
            await DisplayAlert("Éxito", $"Paseo {solicitud.IdSolicitud} iniciado.", "OK");
            
        }

        private async void OnFinalizarPaseoClicked(object sender, EventArgs e)
        {
            try
            {
                IsBusy = true;
                var solicitud = (SolicitudPaseoResponseDto)((Button)sender).CommandParameter;

                if (solicitud == null)
                {
                    await DisplayAlert("Error", "No se encontró la información de la solicitud.", "OK");
                    return;
                }

                string cedulaPaseador = Preferences.Get("CedulaPaseador", string.Empty);

                if (string.IsNullOrEmpty(cedulaPaseador))
                {
                    await DisplayAlert("Error", "No se encontró la cédula del paseador en las preferencias.", "OK");
                    return;
                }

                // Obtener el ID de paseo a partir del ID de solicitud
                var idPaseo = await _apiService.ObtenerIdPaseoPorIdSolicitudAsync(solicitud.IdSolicitud);

                if (!idPaseo.HasValue)
                {
                    await DisplayAlert("Error", "No se encontró un paseo asociado a esta solicitud.", "OK");
                    return;
                }

                var (success, message) = await _apiService.FinalizarPaseoAsync(idPaseo.Value, cedulaPaseador);

                if (success)
                {
                    solicitud.Estado = "Finalizado"; // Usar mayúscula consistentemente
                    ActualizarEstadoBotones(solicitud);
                    GuardarSolicitudes(); // Asegurarse de guardar el nuevo estado
                    await DisplayAlert("Éxito", $"Paseo {idPaseo} finalizado correctamente.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", message, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al finalizar el paseo: {ex}");
                await DisplayAlert("Error", $"Error al finalizar el paseo: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }


        private async Task ResponderSolicitud(SolicitudPaseoResponseDto solicitud, bool aceptada)
        {
            try
            {
                string cedula = Preferences.Get("CedulaPaseador", string.Empty);
                if (string.IsNullOrEmpty(cedula))
                {
                    await DisplayAlert("Error", "No se encontró la cédula del paseador.", "OK");
                    return;
                }

                var resultado = await _apiService.ResponderSolicitudAsync(solicitud.IdSolicitud, cedula, aceptada);
                if (resultado)
                {
                    if (aceptada)
                    {
                        //obtenemos datos desde preferences
                        string nombrePaseador = Preferences.Get("NombrePaseador", string.Empty);
                        string celularPaseador = Preferences.Get("CelularPaseador", string.Empty);

                        var caninos = await _apiService.GetCaninesAsync();
                        var canino = caninos?.FirstOrDefault(c => c.NombreCanino == solicitud.NombreCanino);

                    if (canino != null)
                    {
                            // correctamente para WhatsApp
                            string whatsappLink = $"https://wa.me/+593{celularPaseador}";

                            // Mensaje de notificación con el enlace clickeable
                            string mensajeNotificacion = $"Tu solicitud fue aceptada por {nombrePaseador}. " +
                                                          $"Click para contactar: <a href='{whatsappLink}'><b>{celularPaseador}</b></a>";


                            await _apiService.EnviarNotificacionAsync(
                            canino.IdCanino,
                            mensajeNotificacion,
                            cedula);
                    }


                        await DisplayAlert("Éxito", "Solicitud aceptada. Ahora puedes iniciar el paseo.", "OK");

                        // Actualizar la UI en el hilo principal
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            // Actualizar solo la solicitud aceptada
                            var solicitudExistente = _solicitudes.FirstOrDefault(s => s.IdSolicitud == solicitud.IdSolicitud);
                            if (solicitudExistente != null)
                            {
                                solicitudExistente.Estado = "Aceptada";
                                solicitudExistente.MostrarBotonesRespuesta = false;
                                solicitudExistente.MostrarBotonesPaseo = true;
                                solicitudExistente.PuedeIniciar = true;
                                solicitudExistente.PuedeFinalizar = false;
                            }
                        });
                    }
                    else
                    {
                        // Actualizar solicitud rechazada
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            var solicitudExistente = _solicitudes.FirstOrDefault(s => s.IdSolicitud == solicitud.IdSolicitud);
                            if (solicitudExistente != null)
                            {
                                solicitudExistente.Estado = "Rechazada";
                                solicitudExistente.MostrarBotonesRespuesta = false;
                                solicitudExistente.MostrarBotonesPaseo = false;
                            }
                        });

                        var caninos = await _apiService.GetCaninesAsync();
                        var canino = caninos?.FirstOrDefault(c => c.NombreCanino == solicitud.NombreCanino);

                        if (canino != null)
                        {
                            var notificacionResultado = await _apiService.EnviarNotificacionAsync(
                                canino.IdCanino,
                                "Tu solicitud fue rechazada. Puedes intentarlo con otro paseador.",
                                cedula);

                            if (notificacionResultado.Item1)
                            {
                                await DisplayAlert("Éxito", "Solicitud rechazada y notificación enviada al dueño.", "OK");
                            }
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo procesar la solicitud.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await DisplayAlert("Error", "Ocurrió un error al procesar la solicitud.", "OK");
            }
        }
        private async void OnMenuPaseador_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MenuPaseadorPage());
        }
    }
}
