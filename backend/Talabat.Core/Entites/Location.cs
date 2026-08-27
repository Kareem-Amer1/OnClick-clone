using System;

namespace Talabat.Core.Entites
{
    public class Location
    {
        public Location()
        {
        }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double CalculateDistance(Location other)
        {
            var R = 6371; // Earth's radius in kilometers
            var dLat = ToRad(other.Latitude - Latitude);
            var dLon = ToRad(other.Longitude - Longitude);
            var lat1 = ToRad(Latitude);
            var lat2 = ToRad(other.Latitude);

            var a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
                    Math.Sin(dLon/2) * Math.Sin(dLon/2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
            return R * c;
        }

        private double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
} 