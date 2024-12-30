using System;
using System.Collections.Generic;
using System.Text;

namespace TuProyecto.Models
{
    public class NotificacionDto
    {
        public int IdNotificacion { get; set; }
        public int IdCanino { get; set; }
        public string Mensaje { get; set; }
        public string NumeroPaseador { get; set; }
    }
}
