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
        public decimal distancia_km { get; set; }
        public decimal iniciolatitud { get; set; }

        public decimal iniciolongitud { get; set; }

        public  decimal  finlatitud { get; set; }
        public decimal  finlongitud { get; set; }


        public Canino Canino { get; set; }
    }
}
