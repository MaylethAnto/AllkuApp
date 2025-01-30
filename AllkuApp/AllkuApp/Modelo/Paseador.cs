using System;
using System.Collections.Generic;
using System.Text;

namespace AllkuApp.Modelo
{
    public class Paseador
    {
        public int CedulaPaseador { get; set; }
        public string NombrePaseador { get; set; }
        public string ApellidoPaseador { get; set; }
        public string DireccionPaseador { get; set; }
        public string CelularPaseador { get; set; }
        public string CorreoPaseador { get; set; }

        public bool EstaDisponible { get; set; }
        public int? IdCanino { get; set; }

      
    }
}
