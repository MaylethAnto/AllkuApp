using System;
using System.Collections.Generic;
using System.Text;

namespace AllkuApp.Modelo
{
    public class Gps
    {
        public int IdGps { get; set; }
        public int IdCanino { get; set; }
        public DateTime FechaGps { get; set; }
        public decimal DistanciaKm { get; set; }
        public decimal InicioLatitud { get; set; }

        public decimal InicioLongitud { get; set; }

        public  decimal  FinLatitud { get; set; }
        public decimal  FinLongitud { get; set; }

        public Canino Canino { get; set; }
    }
}
