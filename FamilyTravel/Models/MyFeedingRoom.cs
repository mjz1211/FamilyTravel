using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareTravel.Models
{
    public class MyFeedingRoom
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Tel { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Opentime { get; set; }
        public double Distance { get; set; }
    }
}