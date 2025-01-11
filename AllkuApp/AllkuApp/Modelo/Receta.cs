using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Newtonsoft.Json;

namespace AllkuApp.Modelo
{
    public class Receta
    {
        public int id_receta { get; set; }
        public string nombre_receta { get; set; }
        public string descripcion_receta { get; set; }
        public byte[] foto_receta { get; set; }
        public int id_canino { get; set; }
        public Canino Canino { get; set; }

        // Propiedad adicional para enlazar la imagen en el ListView
        [JsonIgnore]
        public ImageSource FotoReceta { get; set; }
    }
}