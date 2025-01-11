using System;
using System.Collections.Generic;
using System.Text;

namespace AllkuApp.Dtos
{
        public class RegistrarUsuarioDto
        {
            public string Cedula { get; set; } 
            public string Nombre { get; set; }
            public string Usuario { get; set; }
            public string Correo { get; set; } 
            public string Contrasena { get; set; } 
            public string Rol { get; set; } 
            public string Direccion {  get; set; }
            public string Celular { get; set; }


        }
}

