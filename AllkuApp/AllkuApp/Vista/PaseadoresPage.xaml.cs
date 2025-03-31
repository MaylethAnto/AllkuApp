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
        private double? _latitudInicio;
        private double? _longitudInicio;
        private static Dictionary<int, int> IdGpsPorCanino = new Dictionary<int, int>();

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

                nuevasSolicitudes = nuevasSolicitudes.OrderByDescending(s => s.FechaSolicitud).ToList();

                foreach (var nuevaSolicitud in nuevasSolicitudes)
                {
                    var solicitudExistente = _solicitudes.FirstOrDefault(s => s.IdSolicitud == nuevaSolicitud.IdSolicitud);

                    if (solicitudExistente == null)
                    {
                        nuevaSolicitud.MostrarBotonesRespuesta = true;
                        nuevaSolicitud.MostrarBotonesPaseo = false;
                        nuevaSolicitud.PuedeIniciar = false;
                        nuevaSolicitud.PuedeFinalizar = false;

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
                        ActualizarEstadoBotones(solicitudExistente);
                    }
                }

                var solicitudesOrdenadas = new ObservableCollection<SolicitudPaseoResponseDto>(
                    _solicitudes.OrderByDescending(s => s.FechaSolicitud)
                );
                _solicitudes = solicitudesOrdenadas;
                SolicitudesListView.ItemsSource = _solicitudes;

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

        private async Task<Location> ObtenerUbicacionAsync()
        {
            try
            {
                return await Geolocation.GetLastKnownLocationAsync() ??
                       await Geolocation.GetLocationAsync(new GeolocationRequest
                       {
                           DesiredAccuracy = GeolocationAccuracy.High,
                           Timeout = TimeSpan.FromSeconds(10)
                       });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo ubicación: {ex.Message}");
                return null;
            }
        }
        private async void OnIniciarPaseoClicked(object sender, EventArgs e)
        {
            try
            {
                // Validar el objeto sender y CommandParameter
                if (!(sender is Button button) || !(button.CommandParameter is SolicitudPaseoResponseDto solicitud))
                {
                    await DisplayAlert("Error", "No se pudo obtener la información de la solicitud.", "OK");
                    return;
                }

                // Validar ID del canino
                if (solicitud.IdCanino <= 0)
                {
                    Debug.WriteLine($"⚠️ ID de canino no válido: {solicitud.IdCanino}");
                    await DisplayAlert("Error", "El ID del perro no es válido. No se puede iniciar el paseo.", "OK");
                    return;
                }

                // Obtener ubicación con validación
                var ubicacion = await ObtenerUbicacionAsync();
                if (ubicacion == null)
                {
                    await DisplayAlert("Error", "No se pudo obtener la ubicación inicial.", "OK");
                    return;
                }

                // Mostrar datos para depuración
                Debug.WriteLine($"Iniciando paseo para canino ID: {solicitud.IdCanino}");
                Debug.WriteLine($"Ubicación obtenida: Lat={ubicacion.Latitude}, Long={ubicacion.Longitude}");

                // Crear objeto GPS con validación
                var gps = new Gps
                {
                    id_canino = solicitud.IdCanino,
                    fecha_gps = DateTime.UtcNow,
                    iniciolatitud = (decimal)ubicacion.Latitude,
                    iniciolongitud = (decimal)ubicacion.Longitude,
                    finlatitud = 0,
                    finlongitud = 0
                };

                // Validar objeto GPS antes de enviar
                if (gps.id_canino <= 0 || gps.iniciolatitud == 0 || gps.iniciolongitud == 0)
                {
                    Debug.WriteLine($"⚠️ Datos GPS no válidos: Canino={gps.id_canino}, Lat={gps.iniciolatitud}, Long={gps.iniciolongitud}");
                    await DisplayAlert("Error", "Los datos de ubicación no son válidos.", "OK");
                    return;
                }

                // Registrar GPS con manejo de errores mejorado
                var response = await _apiService.RegistrarGpsAsync(gps);

                if (response != null && response.id_gps > 0)
                {
                    // Actualizar estado de la solicitud
                    solicitud.Estado = "En Progreso";
                    solicitud.PuedeIniciar = false;
                    solicitud.PuedeFinalizar = true;
                    solicitud.MostrarBotonesPaseo = true;
                    GuardarSolicitudes();

                    // Guardar ID del GPS
                    IdGpsPorCanino[solicitud.IdCanino] = response.id_gps;
                    Preferences.Set($"IdGps_{solicitud.IdCanino}", response.id_gps.ToString());

                    Debug.WriteLine($"✅ GPS registrado correctamente. ID: {response.id_gps}");
                    await DisplayAlert("Éxito", $"Paseo {solicitud.IdSolicitud} iniciado correctamente.", "OK");

                    // Enviar SMS de inicio
                    var mainActivity = DependencyService.Get<IMainActivityService>();
                    mainActivity?.SendSms("+593981257536", "777");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo registrar el inicio del paseo en el servidor.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔥 Error crítico en OnIniciarPaseoClicked: {ex}");
                await DisplayAlert("Error", $"Error al iniciar paseo: {ex.Message}", "OK");
            }
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

                // Obtener el ID del GPS desde memoria o preferencias
                if (!IdGpsPorCanino.TryGetValue(solicitud.IdCanino, out int idGps))
                {
                    // Si no está en memoria, intentamos obtenerlo de las preferencias
                    var idGpsStr = Preferences.Get($"IdGps_{solicitud.IdCanino}", string.Empty);
                    if (!string.IsNullOrEmpty(idGpsStr) && int.TryParse(idGpsStr, out idGps))
                    {
                        IdGpsPorCanino[solicitud.IdCanino] = idGps;
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se encontró el registro del paseo. Inicia el paseo nuevamente.", "OK");
                        return;
                    }
                }

                var ubicacion = await ObtenerUbicacionAsync();
                if (ubicacion == null)
                {
                    await DisplayAlert("Error", "No se pudo obtener la ubicación final.", "OK");
                    return;
                }

                var gps = new Gps
                {
                    id_gps = idGps,
                    finlatitud = (decimal)ubicacion.Latitude,
                    finlongitud = (decimal)ubicacion.Longitude
                };

                Preferences.Set("LatitudFinal", ubicacion.Latitude.ToString());
                Preferences.Set("LongitudFinal", ubicacion.Longitude.ToString());

                await _apiService.ActualizarGpsAsync(gps);

                solicitud.Estado = "Finalizado";
                solicitud.PuedeFinalizar = false;
                GuardarSolicitudes();

                await DisplayAlert("Éxito", $"Paseo {solicitud.IdSolicitud} finalizado correctamente.", "OK");

                // Enviar SMS de finalización
                var phoneNumber = "+593981257536";
                var mainActivity = DependencyService.Get<IMainActivityService>();
                mainActivity?.SendSms(phoneNumber, "000");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo finalizar el paseo: {ex.Message}", "OK");
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
                        string nombrePaseador = Preferences.Get("NombrePaseador", string.Empty);
                        string celularPaseador = Preferences.Get("CelularPaseador", string.Empty);

                        var caninos = await _apiService.GetCaninesAsync();
                        var canino = caninos?.FirstOrDefault(c => c.NombreCanino == solicitud.NombreCanino);

                        if (canino != null)
                        {
                            string whatsappLink = $"https://wa.me/+593{celularPaseador}";

                            string mensajeNotificacion = $"Tu solicitud fue aceptada por {nombrePaseador}. " +
                             $"Click para contactar: {celularPaseador} ({whatsappLink})";

                            await _apiService.EnviarNotificacionAsync(
                            canino.IdCanino,
                            mensajeNotificacion,
                            cedula);
                        }

                        await DisplayAlert("Éxito", "Solicitud aceptada. Ahora puedes iniciar el paseo.", "OK");

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
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

        private async void OnConectarGpsClicked(object sender, EventArgs e)
        {
            try
            {
                var phoneNumber = "+593981257536";
                var connectMessage = "000";

                var mainActivity = DependencyService.Get<IMainActivityService>();
                mainActivity?.SendSms(phoneNumber, connectMessage);

                await DisplayAlert("Éxito", "GPS conectado correctamente.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al conectar el GPS: {ex.Message}");
                await DisplayAlert("Error", "No se pudo conectar el GPS.", "OK");
            }
        }

        private async void OnMenuPaseador_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MenuPaseadorPage());
        }
    }
}