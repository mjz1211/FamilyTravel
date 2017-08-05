using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareTravel.Models
{
    public class CustomWeather
    {
        public string City { get; set; }
        public string Region { get; set; }
        public List<MyWeatherDescription> WeatherDescription { get; set; }
        public MyAQI AQI { get; set; }
    }

    public class MyWeatherDescription
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Weather{ get; set; }
        public string Rain { get; set; }
        public string Temp { get; set; }
        public string Status { get; set; }
        public string Wind { get; set; }
        public string Humidity { get; set; }
        public string UVI { get; set; }
    }

    public class MyAQI {
        public string City { get; set; }
        public string Region { get; set; }
        public string Status { get; set; }
        public string AQI { get; set; }
        public string PublishTime { get; set; }

    }


}