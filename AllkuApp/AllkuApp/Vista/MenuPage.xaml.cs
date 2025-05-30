﻿using Xamarin.Forms;
using Xamarin.Essentials;
using System;
using AllkuApp.Vista;
using AllkuApp.Services;
using System.Collections.Generic;
using ModeloReceta = AllkuApp.Modelo.Receta;
using ServicioReceta = AllkuApp.Services.Receta;
using CreateRecetaRequest = AllkuApp.Services.CreateRecetaRequest;
using AllkuApp.Modelo;

namespace AllkuApp.Vista
{
    public partial class MenuPage : ContentPage
    {
        private readonly RecetaService _recetaService;
        private List<ModeloReceta> _recetas;


        public MenuPage()
        {
            InitializeComponent();
            _recetaService = new RecetaService();
            _recetas = new List<ModeloReceta>();
            MessagingCenter.Subscribe<RecetaFormulario, ModeloReceta>(this, "RecetaCreada", (sender, receta) =>
            {
                _recetas.Add(receta);
                Console.WriteLine($"Receta agregada: {receta.nombre_receta}");
            });

            // Cargar las recetas al inicializar la página
            CargarRecetas();
        }

        private async void CargarRecetas()
        {
            try
            {
                var recetasServicio = await _recetaService.GetRecetasAsync();
                _recetas = ConvertirRecetas(recetasServicio);
                Console.WriteLine($"Número de recetas cargadas: {_recetas.Count}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un problema al cargar las recetas: {ex.Message}", "OK");
            }
        }

        private List<ModeloReceta> ConvertirRecetas(List<ServicioReceta> recetasServicio)
        {
            var recetasModelo = new List<ModeloReceta>();
            foreach (var receta in recetasServicio)
            {
                recetasModelo.Add(new ModeloReceta
                {
                    nombre_receta = receta.nombre_receta,
                    descripcion_receta = receta.descripcion_receta,
                    foto_receta = receta.foto_receta,
                    id_canino = receta.id_canino
                });
            }

            return recetasModelo;
        }


        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navegar de regreso a la página anterior
            await Navigation.PopAsync();
        }
        private void OnAddPetClicked(object sender, EventArgs e)
        {
            // Navegar a la página para agregar mascota
            Navigation.PushAsync(new RegistroMascotaPage());
        }

        private void OnPerfilClicked(object sender, EventArgs e)
        {
            // Navegar a la página de perfil
            Navigation.PushAsync(new PerfilPage());
        }

        private void OnEjerciciosClicked(object sender, EventArgs e)
        {
            // Navegar a la página de ejercicios
            Navigation.PushAsync(new EjerciciosPage());
        }

        private async void OnRecetasClicked(object sender, EventArgs e)
        {
            // Navegar a la página que muestra las recetas
            await Navigation.PushAsync(new RecetaPage());
        }

        private async void OnHistorialClinicoClicked(object sender, EventArgs e)
        {
            try
            {
                // Obtener el ID del canino desde Preferences que fue guardado en CargarDatosCanino
                int idCanino = Preferences.Get("CaninoId", 0);

                if (idCanino == 0)
                {
                    await DisplayAlert("Error", "No se pudo obtener la información del canino", "OK");
                    return;
                }

                // Navegar directamente a la página de crear historial
                await Navigation.PushAsync(new CrearHistorialPage(idCanino));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }


        private void OnGPSClicked(object sender, EventArgs e)
        {
            // Navegar a la página de GPS
            Navigation.PushAsync(new GPSPage());
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            // Cerrar sesión y navegar a la página de inicio de sesión
            SecureStorage.Remove("user_token");
            SecureStorage.Remove("user_id");
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}