using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareTravel.Models
{
    public class CustomAQI
    {
        public string City { get; set; }
        public string Region { get; set; }
        public double RLat { get; set; }
        public double RLng { get; set; }
        public string AQI { get; set; }
        public string Pollutant { get; set; }
        public string Status { get; set; }
        public string SO2 { get; set; }
        public string CO { get; set; }
        public string CO_8hr { get; set; }
        public string O3 { get; set; }
        public string O3_8hr { get; set; }
        public string PM10 { get; set; }
        public string PM25 { get; set; }
        public string NO2 { get; set; }
        public string NOx { get; set; }
        public string NO { get; set; }
        public string WindSpeed { get; set; }
        public string WindDirec { get; set; }
        public string PublishTime { get; set; }
        public string PM25_AVG { get; set; }
        public string PM10_AVG { get; set; }
    }
}