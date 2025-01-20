using System;
using System.Collections.Generic;
using System.Text;

namespace AllkuApp.Modelo
{
    public class PaseoModel
    {
        public int IdPaseo { get; set; }
        public int IdSolicitud { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public double? DistanciaKm { get; set; }
        public string EstadoPaseo { get; set; }

    }
}
