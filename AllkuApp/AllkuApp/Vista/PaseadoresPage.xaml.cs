﻿using AllkuApp.Dtos;
using AllkuApp.Modelo;
using AllkuApp.Servicios;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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

                // Obtenemos las solicitudes del paseador
                var nuevasSolicitudes = await _apiService.GetSolicitudesPaseadorAsync(cedula);
                Debug.WriteLine($"Solicitudes obtenidas de la API: {nuevasSolicitudes.Count}");

                // Ya no filtramos por CedulaPaseador, asumimos que la API ya hizo ese filtro
                var solicitudesOrdenadas = nuevasSolicitudes.OrderByDescending(s => s.FechaSolicitud).ToList();

                if (solicitudesOrdenadas.Count == 0)
                {
                    // Mostrar un mensaje cuando no hay solicitudes
                    await DisplayAlert("Información", "No tienes solicitudes pendientes.", "OK");
                    _solicitudes.Clear();
                    SolicitudesListView.ItemsSource = _solicitudes;
                    return;
                }

                // Actualizar la colección existente
                foreach (var nuevaSolicitud in solicitudesOrdenadas)
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

                // Actualizar el ListView
                var nuevaColeccion = new ObservableCollection<SolicitudPaseoResponseDto>(
                    _solicitudes.OrderByDescending(s => s.FechaSolicitud)
                );
                _solicitudes = nuevaColeccion;
                SolicitudesListView.ItemsSource = _solicitudes;
                GuardarSolicitudes();

                Debug.WriteLine($"Total de solicitudes en _solicitudes: {_solicitudes.Count}");
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


        // Calcula la distancia en kilómetros entre dos coordenadas GPS usando la fórmula de Haversine
        private double CalcularDistanciaKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double RADIO_TIERRA = 6371.0; // Radio medio de la Tierra en kilómetros

            // Convertir a radianes
            double latitud1 = lat1 * Math.PI / 180.0;
            double longitud1 = lon1 * Math.PI / 180.0;
            double latitud2 = lat2 * Math.PI / 180.0;
            double longitud2 = lon2 * Math.PI / 180.0;

            // Diferencias
            double dLat = latitud2 - latitud1;
            double dLon = longitud2 - longitud1;

            // Fórmula de Haversine
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(latitud1) * Math.Cos(latitud2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distancia = RADIO_TIERRA * c;

            return Math.Round(distancia, 2);
        }

        private async void OnIniciarPaseoClicked(object sender, EventArgs e)
        {
            try
            {
                // Mostrar indicador de carga
                IsBusy = true;

                // Validar el objeto sender y CommandParameter
                if (!(sender is Button button) || !(button.CommandParameter is SolicitudPaseoResponseDto solicitud))
                {
                    await DisplayAlert("Error", "No se pudo obtener la información de la solicitud.", "OK");
                    return;
                }

                // Validar ID del canino
                if (solicitud.IdCanino <= 0)
                {
                    Debug.WriteLine($"⚠️ ID de canino no válido: {solicitud.IdCanino}, intentando obtener por nombre");

                    // Obtener el ID del canino por nombre
                    int idCanino = await _apiService.ObtenerIdCaninoPorNombreAsync(solicitud.NombreCanino);

                    if (idCanino <= 0)
                    {
                        Debug.WriteLine($"⚠️ No se pudo obtener el ID del canino para: {solicitud.NombreCanino}");
                        await DisplayAlert("Error", "No se pudo obtener el ID del perro. No se puede iniciar el paseo.", "OK");
                        return;
                    }

                    // Asignar el ID obtenido
                    solicitud.IdCanino = idCanino;
                    Debug.WriteLine($"✅ ID del canino obtenido: {idCanino} para {solicitud.NombreCanino}");
                }

                // Obtener ubicación con validación
                var ubicacion = await ObtenerUbicacionAsync();
                if (ubicacion == null)
                {
                    await DisplayAlert("Error", "No se pudo obtener la ubicación inicial.", "OK");
                    return;
                }

                // Mostrar datos para depuración
                Debug.WriteLine($"Iniciando paseo para canino ID: {solicitud.IdCanino}, Nombre: {solicitud.NombreCanino}");
                Debug.WriteLine($"Ubicación obtenida: Lat={ubicacion.Latitude}, Long={ubicacion.Longitude}");

                // Crear objeto GPS con validación
                var gps = new Gps
                {
                    IdCanino = solicitud.IdCanino,
                    FechaGps = DateTime.UtcNow,
                    InicioLatitud = (decimal)ubicacion.Latitude,
                    InicioLongitud = (decimal)ubicacion.Longitude,
                    FinLatitud = 0,
                    FinLongitud = 0
                };

                // GUARDAR COORDENADAS INICIALES (AÑADE ESTO)
                Preferences.Set($"InicioLatitud_{solicitud.IdCanino}", ubicacion.Latitude.ToString(CultureInfo.InvariantCulture));
                Preferences.Set($"InicioLongitud_{solicitud.IdCanino}", ubicacion.Longitude.ToString(CultureInfo.InvariantCulture));
                Debug.WriteLine($"📌 Coordenadas iniciales guardadas: {ubicacion.Latitude}, {ubicacion.Longitude}");

                // Validar objeto GPS antes de enviar
                if (gps.IdCanino <= 0 || gps.InicioLatitud == 0 || gps.InicioLongitud == 0)
                {
                    Debug.WriteLine($"⚠️ Datos GPS no válidos: Canino={gps.IdCanino}, Lat={gps.InicioLatitud}, Long={gps.InicioLongitud}");
                    await DisplayAlert("Error", "Los datos de ubicación no son válidos.", "OK");
                    return;
                }

                // Registrar GPS con manejo de errores mejorado
                var response = await _apiService.RegistrarGpsAsync(gps);

                if (response != null && response.IdGps > 0)
                {
                    // Actualizar estado de la solicitud
                    solicitud.Estado = "En Progreso";
                    solicitud.PuedeIniciar = false;
                    solicitud.PuedeFinalizar = true;
                    solicitud.MostrarBotonesPaseo = true;
                    GuardarSolicitudes();

                    // Guardar ID del GPS
                    IdGpsPorCanino[solicitud.IdCanino] = response.IdGps;
                    Preferences.Set($"IdGps_{solicitud.IdCanino}", response.IdGps.ToString());

                    Debug.WriteLine($"✅ GPS registrado correctamente. ID: {response.IdGps}");
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
            finally
            {
                // Ocultar indicador de carga
                IsBusy = false;
            }
        }

        private async void OnFinalizarPaseoClicked(object sender, EventArgs e)
        {
            try
            {
                IsBusy = true;

                // Validar el objeto sender y CommandParameter
                if (!(sender is Button button) || !(button.CommandParameter is SolicitudPaseoResponseDto solicitud))
                {
                    await DisplayAlert("Error", "No se pudo obtener la información de la solicitud.", "OK");
                    return;
                }

                Debug.WriteLine($"Finalizando paseo para canino ID: {solicitud.IdCanino}, Nombre: {solicitud.NombreCanino}");

                // Obtener el ID del GPS desde memoria o preferencias
                if (!IdGpsPorCanino.TryGetValue(solicitud.IdCanino, out int idGps))
                {
                    // Si no está en memoria, intentamos obtenerlo de las preferencias
                    var idGpsStr = Preferences.Get($"IdGps_{solicitud.IdCanino}", string.Empty);
                    if (!string.IsNullOrEmpty(idGpsStr) && int.TryParse(idGpsStr, out idGps))
                    {
                        IdGpsPorCanino[solicitud.IdCanino] = idGps;
                        Debug.WriteLine($"ID GPS recuperado de preferencias: {idGps}");
                    }
                    else
                    {
                        Debug.WriteLine("⚠️ No se encontró el ID del GPS");
                        await DisplayAlert("Error", "No se encontró el registro del paseo. Inicia el paseo nuevamente.", "OK");
                        return;
                    }
                }

                // Obtener ubicación final con validación
                var ubicacionFinal = await ObtenerUbicacionAsync();
                if (ubicacionFinal == null)
                {
                    await DisplayAlert("Error", "No se pudo obtener la ubicación final.", "OK");
                    return;
                }

                Debug.WriteLine($"Ubicación final obtenida: Lat={ubicacionFinal.Latitude}, Long={ubicacionFinal.Longitude}");

                // Recuperar las coordenadas iniciales de las preferencias
                double inicioLatitud = 0;
                double inicioLongitud = 0;

                // Recuperar con CultureInfo.InvariantCulture
                var latStr = Preferences.Get($"InicioLatitud_{solicitud.IdCanino}", null);
                var lonStr = Preferences.Get($"InicioLongitud_{solicitud.IdCanino}", null);

                if (!string.IsNullOrEmpty(latStr))
                    double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out inicioLatitud);

                if (!string.IsNullOrEmpty(lonStr))
                    double.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out inicioLongitud);

                if (inicioLatitud == 0 || inicioLongitud == 0)
                {
                    Debug.WriteLine("⚠️ No se encontraron coordenadas iniciales guardadas");
                    await DisplayAlert("Advertencia", "No se encontró el punto de inicio del paseo", "OK");
                    return;
                }

                Debug.WriteLine($"📍 Coordenadas iniciales recuperadas: Lat={inicioLatitud}, Lon={inicioLongitud}");
            

                // Crear objeto GPS con ubicación final
                var gps = new Gps
                {
                    IdGps = idGps,
                    FinLatitud = (decimal)ubicacionFinal.Latitude,
                    FinLongitud = (decimal)ubicacionFinal.Longitude
                };

                // Calcular distancia si tenemos las coordenadas iniciales
                double distanciaKm = CalcularDistanciaKm(
                     inicioLatitud,
                     inicioLongitud,
                     ubicacionFinal.Latitude,
                     ubicacionFinal.Longitude
                 );

                Debug.WriteLine($"📏 Distancia calculada: {distanciaKm} km");

                // Guardar coordenadas finales en preferencias
                Preferences.Set("LatitudFinal", ubicacionFinal.Latitude.ToString());
                Preferences.Set("LongitudFinal", ubicacionFinal.Longitude.ToString());

                // Actualizar GPS en la API
                try
                {
                    await _apiService.ActualizarGpsAsync(gps);

                    // Actualizar estado de la solicitud
                    solicitud.Estado = "Finalizado";
                    solicitud.PuedeFinalizar = false;
                    solicitud.MostrarBotonesPaseo = false;
                    GuardarSolicitudes();

                    // Eliminar ID del GPS de la memoria y preferencias
                    IdGpsPorCanino.Remove(solicitud.IdCanino);
                    Preferences.Remove($"IdGps_{solicitud.IdCanino}");
                    Preferences.Remove($"InicioLatitud_{idGps}");
                    Preferences.Remove($"InicioLongitud_{idGps}");

                    string mensajeDistancia = distanciaKm > 0
                        ? $"\nDistancia recorrida: {distanciaKm:F2} km"
                        : "";

                    await DisplayAlert("Éxito", $"Paseo finalizado correctamente.{mensajeDistancia}", "OK");

                    // Enviar SMS de finalización
                    var phoneNumber = "+593981257536";
                    var mainActivity = DependencyService.Get<IMainActivityService>();
                    mainActivity?.SendSms(phoneNumber, "000");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al actualizar GPS: {ex.Message}");
                    await DisplayAlert("Error", $"No se pudo finalizar el paseo: {ex.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔥 Error crítico en OnFinalizarPaseoClicked: {ex}");
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