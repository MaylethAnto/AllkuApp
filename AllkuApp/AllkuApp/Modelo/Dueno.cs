using System;
using System.Collections.Generic;
using System.Text;

namespace AllkuApp.Modelo
{
    public class Dueno
    {
        public string CedulaDueno { get; set; } = null;
        public string NombreDueno { get; set; }
        public string DireccionDueno { get; set; }
        public string CelularDueno { get; set; }
        public string CorreoDueno { get; set; }

        public ICollection<Canino> Caninos { get; set; }

    }
}
