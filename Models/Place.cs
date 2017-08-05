using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareTravel.Models
{
    public class Place
    {
        public string Place_Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public double Distance { get; set; }
        public double Rating { get; set; }
        public string Address { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
        public string PType { get; set; }
        public string Weather { get; set; }
        public string Rain { get; set; }
        public string Temp { get; set; }
        public string Wind { get; set; }
        public string Humidity { get; set; }
        public string Status { get; set; }
        public string UVI { get; set; }
        public string AQI { get; set; }
    }
}