using System;
using System.Collections.Generic;
using System.Text;



namespace AllkuApp.Modelo
{
    public class RecetaRequest
    {

        public int id_receta { get; set; }

        public string nombre_receta { get; set; }

        public string descripcion_receta { get; set; }

        public byte[] foto_receta { get; set; }

        public int id_canino { get; set; }
    }
}