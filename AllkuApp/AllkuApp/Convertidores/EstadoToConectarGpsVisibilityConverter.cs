using System;
using System.Globalization;
using Xamarin.Forms;

namespace AllkuApp.Convertidores
{
    public class EstadoToConectarGpsVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            string estado = value.ToString().ToLower();
            return estado == "aceptada";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}