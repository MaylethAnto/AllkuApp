using System;
using System.Collections.Generic;
using System.Text;

namespace AllkuApp.Servicios
{
    public interface IMainActivityService
    {
        void SendSms(string phoneNumber, string message);
    }
}
