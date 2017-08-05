using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareTravel.Models
{
    public static class GeolocationUtils
    {
        public enum DistanceUnit
        {
            Miles,
            Kilometers,
            NauticalMiles
        }

        /// <summary>
        /// Gets the distance.
        /// </summary>
        /// <returns>The distance.</returns>
        /// <param name="lat1">Lat1.</param>
        /// <param name="lon1">Lon1.</param>
        /// <param name="lat2">Lat2.</param>
        /// <param name="lon2">Lon2.</param>
        /// <param name="unit">Unit.</param>
        public static double GetDistance(double lat1, double lon1, double lat2, double lon2, DistanceUnit unit = DistanceUnit.Miles)
        {
            var theta = lon1 - lon2;
            var dist = Math.Sin(DegreesToRadians(lat1)) * Math.Sin(DegreesToRadians(lat2)) + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) * Math.Cos(DegreesToRadians(theta));
            dist = Math.Acos(dist);
            dist = RadiansToDegrees(dist);
            dist = dist * 60 * 1.1515;//miles

            switch (unit)
            {
                case DistanceUnit.Kilometers:
                    dist *= 1.609344;
                    break;
                case DistanceUnit.NauticalMiles:
                    dist *= 0.8684;
                    break;
            }

            return (dist);
        }

        /// <summary>
        /// Degreeses to radians.
        /// </summary>
        /// <returns>The to radians.</returns>
        /// <param name="deg">Degrees.</param>
        private static double DegreesToRadians(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        /// <summary>
        /// Radianses to degrees.
        /// </summary>
        /// <returns>The to degrees.</returns>
        /// <param name="rad">Radians.</param>
        private static double RadiansToDegrees(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
    }
}