using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AllkuApp.Dtos
{
    public class SolicitudPaseoResponseDto : INotifyPropertyChanged
    {
        private int _idSolicitud;
        private int _idCanino;
        private string _nombreCanino;
        private string _nombreDueno;
        private string _celularDueno;
        private DateTime _fechaSolicitud;
        private bool _mostrarBotonesRespuesta;
        private bool _mostrarBotonesPaseo;
        private bool _puedeIniciar;
        private bool _puedeFinalizar;
        private string _estado = "Pendiente";
        private string _cedulaPaseador; // Nueva propiedad

        public int IdSolicitud
        {
            get => _idSolicitud;
            set
            {
                _idSolicitud = value;
                OnPropertyChanged();
            }
        }

        public int IdCanino
        {
            get => _idCanino;
            set
            {
                _idCanino = value;
                OnPropertyChanged();
            }
        }

        public string NombreCanino
        {
            get => _nombreCanino;
            set
            {
                _nombreCanino = value;
                OnPropertyChanged();
            }
        }

        public string NombreDueno
        {
            get => _nombreDueno;
            set
            {
                _nombreDueno = value;
                OnPropertyChanged();
            }
        }

        public string CelularDueno
        {
            get => _celularDueno;
            set
            {
                _celularDueno = value;
                OnPropertyChanged();
            }
        }

        public DateTime FechaSolicitud
        {
            get => _fechaSolicitud;
            set
            {
                _fechaSolicitud = value;
                OnPropertyChanged();
            }
        }

        public bool MostrarBotonesRespuesta
        {
            get => _mostrarBotonesRespuesta;
            set
            {
                _mostrarBotonesRespuesta = value;
                OnPropertyChanged();
            }
        }

        public bool MostrarBotonesPaseo
        {
            get => _mostrarBotonesPaseo;
            set
            {
                _mostrarBotonesPaseo = value;
                OnPropertyChanged();
            }
        }

        public bool PuedeIniciar
        {
            get => _puedeIniciar;
            set
            {
                _puedeIniciar = value;
                OnPropertyChanged();
            }
        }

        public bool PuedeFinalizar
        {
            get => _puedeFinalizar;
            set
            {
                _puedeFinalizar = value;
                OnPropertyChanged();
            }
        }

        public string Estado
        {
            get => _estado;
            set
            {
                _estado = value;
                OnPropertyChanged();
            }
        }

        public string CedulaPaseador // Nueva propiedad
        {
            get => _cedulaPaseador;
            set
            {
                _cedulaPaseador = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}