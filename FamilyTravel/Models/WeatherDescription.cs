using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ShareTravel.Models
{

    public class WeatherDescription
    {
        [JsonProperty("success")]
        public string success { get; set; }
        [JsonProperty("result")]
        public Result result { get; set; }
        [JsonProperty("records")]
        public Records records { get; set; }
       
    }

    public class Result
    {
        [JsonProperty("resource_id")]
        public string resource_id { get; set; }
        [JsonProperty("fields")]
        public List<Field> fields { get; set; }
    }

    public class Field
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("type")]
        public string type { get; set; }
    }

    public class Records
    {
        [JsonProperty("contentDescription")]
        public string contentDescription { get; set; }
        [JsonProperty("locations")]
        public List<Location> locations { get; set; }
    }

    public class Location
    {
        [JsonProperty("datasetDescription")]
        public string datasetDescription { get; set; }
        [JsonProperty("locationsName")]
        public string locationsName { get; set; }
        [JsonProperty("dataid")]
        public string dataid { get; set; }
        [JsonProperty("location")]
        public List<Location2> location { get; set; }
    }

    public class Location2
    {
        [JsonProperty("locationName")]
        public string locationName { get; set; }
        [JsonProperty("geocode")]
        public string geocode { get; set; }
        [JsonProperty("lat")]
        public string lat { get; set; }
        [JsonProperty("lon")]
        public string lon { get; set; }
        [JsonProperty("weatherElement")]
        public List<WeatherElement> weatherElement { get; set; }
    }

    public class WeatherElement
    {
        [JsonProperty("elementName")]
        public string elementName { get; set; }
        [JsonProperty("time")]
        public List<Time> time { get; set; }
    }

    public class Time
    {
        [JsonProperty("startTime")]
        public string startTime { get; set; }
        [JsonProperty("endTime")]
        public string endTime { get; set; }
        [JsonProperty("elementValue")]
        public string elementValue { get; set; }
        [JsonProperty("parameter")]
        public List<Parameter> parameter { get; set; }
    }

    public class Parameter
    {
        [JsonProperty("parameterName")]
        public string parameterName { get; set; }
        [JsonProperty("parameterValue")]
        public string parameterValue { get; set; }
    }
}