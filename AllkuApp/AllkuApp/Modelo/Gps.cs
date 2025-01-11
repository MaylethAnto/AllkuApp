using System;
using System.Collections.Generic;
using System.Text;

namespace AllkuApp.Modelo
{
    public class Gps
    {
        public int id_gps { get; set; }
        public int id_canino { get; set; }
        public DateTime fecha_gps { get; set; }
        public string ubicacion_gps { get; set; }
        public int pasos_gps { get; set; }
        public decimal distancia_km { get; set; }

        public Canino Canino { get; set; }
    }
}
