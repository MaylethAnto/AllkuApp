using AllkuApp.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllkuApp.Vista
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PaseadorSelectionPage : ContentPage
    {
        public event EventHandler<PaseadorDisponibleDto> PaseadorSeleccionado;

        public PaseadorSelectionPage(List<PaseadorDisponibleDto> paseadores)
        {
            InitializeComponent();
            PaseadoresListView.ItemsSource = paseadores;
        }

        private void OnPaseadorSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is PaseadorDisponibleDto paseador)
            {
                PaseadorSeleccionado?.Invoke(this, paseador);
                Navigation.PopModalAsync();
            }
        }
    }
}