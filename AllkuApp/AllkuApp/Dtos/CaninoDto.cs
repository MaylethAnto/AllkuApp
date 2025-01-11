using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace AllkuApp.Dtos
{
    public class CaninoDto
    {
        public int IdCanino { get; set; }
        public string NombreCanino { get; set; }
        public int EdadCanino { get; set; }
        public string RazaCanino { get; set; }
        public double PesoCanino { get; set; }
        public byte[] FotoCanino { get; set; }
        public string CelularDueno { get; set; }



        // Propiedad para ImageSource
        public ImageSource FotoCaninoImageSource { get; set; }

    }
}
