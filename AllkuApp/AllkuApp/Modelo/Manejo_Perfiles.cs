using System;
using System.Collections.Generic;
using System.Text;

namespace AllkuApp.Modelo
{
    public class Manejo_Perfiles
    {
        public int id_perfil { get; set; }
        public string nombre_usuario { get; set; }
        public string contrasena { get; set; }
        public string tipo_perfil { get; set; }
        public int id_administrador { get; set; }
        public string cedula_dueno { get; set; }
        public int id_paseador { get; set; }
        public  Administrador Administrador { get; set; }
        public  Dueno Dueno { get; set; }
        public  Paseador Paseador { get; set; }
    }
}
