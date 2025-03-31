using System;
using System.Collections.Generic;
using System.Text;

namespace AllkuApp.Servicios
{
    public interface ISmsService
    {
        void StartSmsListener();
        void StopSmsListener();
    }
}
