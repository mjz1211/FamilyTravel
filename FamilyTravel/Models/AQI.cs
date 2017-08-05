using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;


namespace ShareTravel.Models
{
    public class AQI
    {
        [JsonProperty("success")]
        public bool success { get; set; }
        [JsonProperty("result")]
        public AQIResult result { get; set; }
    }

    public class AQIResult
    {
        [JsonProperty("resource_id")]
        public string resource_id { get; set; }
        [JsonProperty("fields")]
        public List<AQIField> fields { get; set; }
        [JsonProperty("records")]
        public List<AQIRecord> records { get; set; }
        [JsonProperty("limit")]
        public int limit { get; set; }
        [JsonProperty("offset")]
        public int offset { get; set; }
        [JsonProperty("total")]
        public int total { get; set; }
    }


    public class AQIField
    {
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("id")]
        public string id { get; set; }
    }


    public class AQIRecord
    {
        [JsonProperty("SiteName")]
        public string SiteName { get; set; }
        [JsonProperty("County")]
        public string County { get; set; }
        [JsonProperty("AQI")]
        public string AQI { get; set; }
        [JsonProperty("Pollutant")]
        public string Pollutant { get; set; }
        [JsonProperty("Status")]
        public string Status { get; set; }
        [JsonProperty("SO2")]
        public string SO2 { get; set; }
        [JsonProperty("CO")]
        public string CO { get; set; }
        [JsonProperty("CO_8hr")]
        public string CO_8hr { get; set; }
        [JsonProperty("O3")]
        public string O3 { get; set; }
        [JsonProperty("O3_8hr")]
        public string O3_8hr { get; set; }
        [JsonProperty("PM10")]
        public string PM10 { get; set; }
        [JsonProperty("PM2.5")]
        public string PM25 { get; set; }
        [JsonProperty("NO2")]
        public string NO2 { get; set; }
        [JsonProperty("NOx")]
        public string NOx { get; set; }
        [JsonProperty("NO")]
        public string NO { get; set; }
        [JsonProperty("WindSpeed")]
        public string WindSpeed { get; set; }
        [JsonProperty("WindDirec")]
        public string WindDirec { get; set; }
        [JsonProperty("PublishTime")]
        public string PublishTime { get; set; }
        [JsonProperty("PM2.5_AVG")]
        public string PM25_AVG { get; set; }
        [JsonProperty("PM10_AVG")]
        public string PM10_AVG { get; set; }
    }
}