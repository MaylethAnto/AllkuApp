using System;

namespace AllkuApp.Modelo
{
    public class DistanciaModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public double LatitudInicio { get; set; }
        public double LongitudInicio { get; set; }
        public double LatitudFin { get; set; }
        public double LongitudFin { get; set; }
        public double DistanciaTotal => CalcularDistancia();

        private double CalcularDistancia()
        {
            var R = 6371; // Radio de la Tierra en kilómetros
            var dLat = ToRadians(LatitudFin - LatitudInicio);
            var dLon = ToRadians(LongitudFin - LongitudInicio);
            var lat1 = ToRadians(LatitudInicio);
            var lat2 = ToRadians(LatitudFin);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Distancia en kilómetros
        }

        private double ToRadians(double angle)
        {
            return angle * Math.PI / 180;
        }
    }
}