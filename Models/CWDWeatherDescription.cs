//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ShareTravel.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CWDWeatherDescription
    {
        public int WD_Id { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public double RLat { get; set; }
        public double RLng { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public System.DateTime ModifyTime { get; set; }
        public string Weather { get; set; }
        public string Rain { get; set; }
        public string Temp { get; set; }
        public string Status { get; set; }
        public string Wind { get; set; }
        public string Humidity { get; set; }
    }
}
