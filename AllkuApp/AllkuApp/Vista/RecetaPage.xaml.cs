using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AllkuApp.Modelo;
using AllkuApp.Servicios;
using AllkuApp.Services;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecetaPage : ContentPage
    {
        private readonly RecetaService _recetaService;
        public RecetaPage(List<CreateRecetaRequest> recetas)
        {
            InitializeComponent();
            RecetasListView.ItemsSource = recetas;
        }
    }
}