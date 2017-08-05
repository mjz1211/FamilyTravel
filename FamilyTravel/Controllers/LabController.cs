using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;

using System.Threading.Tasks;
using Newtonsoft.Json;

using ShareTravel.Models;


namespace ShareTravel.Controllers
{
    public class LabController : Controller
    {
        // GET: Lab
        public ActionResult CWB()
        {
            return View();
        }

        public ActionResult AddCWB() {


            return View();
        }

        public string QueryWeather(string uLat, string uLng)
        {
            string json = "";

            var wdArr = db.CWDWeatherDescription.ToArray();
            var uviArr = db.CWDUVI.ToArray();
            var aqiArr = db.EPAAQI.ToArray();

            var cityRegion = db.CWBCityRegionLocation.ToArray();
            var epaCityRegion = db.EPACityRegionLocation.ToArray();
                             
            var result = from s in cityRegion                         
                         select new{
                            Distance =  GeolocationUtils.GetDistance(s.RLat,s.RLng,Convert.ToDouble(uLat),Convert.ToDouble(uLng)),
                            City = s.City,
                            Region = s.Region
                         };

            result = result.OrderBy(c => c.Distance);

            string city = result.First().City;
            string region = result.First().Region;

            var wdResult = from s in wdArr
                           join o in uviArr on
                           new { s.City, s.Region, s.StartTime, s.EndTime } equals new { o.City, o.Region, o.StartTime, o.EndTime } into details
                           from o in details.DefaultIfEmpty(new CWDUVI
                           {
                               UVI = 0,
                               Exposure = ""
                           }) 
                           where (s.StartTime >= Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"))) &&
                                (s.City == city) &&
                                (s.Region == region)
                           orderby s.StartTime

                           select new MyWeatherDescription {
                               StartTime = s.StartTime.ToString("MM/dd HH:mm"),
                               EndTime = s.EndTime.ToString("MM/dd HH:mm"),
                               Weather = s.Weather,
                               Rain = s.Rain,
                               Temp = "  " + s.Temp.Substring(4, 2) + " - " + s.Temp.Substring(7, 2),
                               Status = s.Status,
                               Wind = s.Wind,
                               Humidity = s.Humidity,
                               UVI = o.UVI + "(" + o.Exposure + ")"
                           };
            //query EPA data
            var result2 = from s in epaCityRegion
                         select new
                         {
                             Distance = GeolocationUtils.GetDistance(s.RLat, s.RLng, Convert.ToDouble(uLat), Convert.ToDouble(uLng)),
                             City = s.City,
                             Region = s.Region
                         };

            result2 = result2.OrderBy(c => c.Distance);

            string city2 = result2.First().City;
            string region2 = result2.First().Region;

            var epaResult = from s in aqiArr
                            where s.City == city2 && s.Region == region2
                            select new MyAQI {
                                City = s.City,
                                Region = s.Region,
                                AQI = s.AQI,
                                Status = s.Status,
                                PublishTime = s.PublishTime.ToString("yyyy/MM/dd")                            
                            };

            CustomWeather cw = new CustomWeather {
                City = city,
                Region = region,
                WeatherDescription =  wdResult.ToList(),
                AQI = epaResult.First()
            };

            json = JsonConvert.SerializeObject(cw);
            return json;
        }

        openTCLEntities db = new openTCLEntities();
        public async Task<string> Poll(string uCity)
        {

            string responseStr = "";
            DateTime t1 = DateTime.Now;

            try
            {
                db.Database.ExecuteSqlCommand("DELETE FROM CWDUVI");
                db.Database.ExecuteSqlCommand("DELETE FROM CWDWeatherDescription");

                string[] cities = { "003","007", "011", "015", "019",
                                    "023","027", "031", "035", "039",
                                    "043","047", "051", "055", "059",
                                    "063","067", "071", "075", "079",
                                    "083","087"};

                foreach (var cItem in cities)
                {
                    


                    string cwbUrl = "https://opendata.cwb.gov.tw/api/v1/rest/datastore/F-D0047-" + cItem + "?elementName=WeatherDescription,UVI&Authorization=CWB-AC9EB733-E679-42D8-8533-82291A93E58D";
                    if( uCity != null)
                        cwbUrl = "https://opendata.cwb.gov.tw/api/v1/rest/datastore/F-D0047-" + uCity + "?elementName=WeatherDescription,UVI&Authorization=CWB-AC9EB733-E679-42D8-8533-82291A93E58D";

                    HttpClient client = new HttpClient();
                    //client.BaseAddress = new Uri(cwbUrl);
                    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                    //client.DefaultRequestHeaders.Add("Authorization", "CWB-AC9EB733-E679-42D8-8533-82291A93E58D");
                    //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/v1/rest/datastore/F-D0047-063?elementName=WeatherDescription&Authorization=CWB-AC9EB733-E679-42D8-8533-82291A93E58D");
                    //HttpResponseMessage x = await client.GetAsync(cwbUrl);
                    string result = await client.GetStringAsync(cwbUrl);

                    List<Location> locations = JsonConvert.DeserializeObject<ShareTravel.Models.WeatherDescription>(result).records.locations;


                    foreach (var item in locations)
                    {
                        foreach (var item2 in item.location)
                        {
                            foreach (var item3 in item2.weatherElement)
                            {
                                foreach (var item4 in item3.time)
                                {
                                    if (item4.elementValue != null)
                                    {


                                        string[] wArr = item4.elementValue.Split('。');
                                        string weather = "", rain = "", temp = "", status = "", wind = "", humidity = "";
                                        if (wArr.Length == 6)
                                        { //no rain
                                            for (int i = 0; i <= wArr.Length - 2; i++)
                                            {
                                                switch (i)
                                                {
                                                    case 0:
                                                        weather = wArr[0];
                                                        break;
                                                    case 1:
                                                        temp = wArr[1].Trim();
                                                        break;
                                                    case 2:
                                                        status = wArr[2].Trim();
                                                        break;
                                                    case 3:
                                                        wind = wArr[3];
                                                        break;
                                                    case 4:
                                                        humidity = wArr[4];
                                                        break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            for (int i = 0; i <= wArr.Length - 2; i++)
                                            {
                                                switch (i)
                                                {
                                                    case 0:
                                                        weather = wArr[0];
                                                        break;
                                                    case 1:
                                                        rain = wArr[1].Trim();
                                                        break;
                                                    case 2:
                                                        temp = wArr[2].Trim();
                                                        break;
                                                    case 3:
                                                        status = wArr[3].Trim();
                                                        break;
                                                    case 4:
                                                        wind = wArr[4];
                                                        break;
                                                    case 5:
                                                        humidity = wArr[5];
                                                        break;
                                                }
                                            }
                                        }


                                        CWDWeatherDescription cwdData = new CWDWeatherDescription
                                        {
                                            City = item.locationsName,
                                            Region = item2.locationName,
                                            RLat = Convert.ToDouble(item2.lat),
                                            RLng = Convert.ToDouble(item2.lon),
                                            StartTime = Convert.ToDateTime(item4.startTime),
                                            EndTime = Convert.ToDateTime(item4.endTime),
                                            ModifyTime = DateTime.Now,
                                            Weather = weather,
                                            Rain = rain,
                                            Temp = temp,
                                            Status = status,
                                            Wind = wind,
                                            Humidity = humidity
                                        };

                                        //insert into DB
                                        db.CWDWeatherDescription.Add(cwdData);
                                    }
                                    else if (item4.parameter != null)
                                    {
                                        int uvi = 0;
                                        string exposure = "";
                                        int j = 0;
                                        foreach (var item5 in item4.parameter)
                                        {


                                            if (j == 0)
                                            {
                                                uvi = Convert.ToInt16(item5.parameterValue);
                                            }
                                            else
                                            {
                                                exposure = item5.parameterValue;
                                            }
                                            j++;

                                        }
                                        CWDUVI cwdData = new CWDUVI
                                        {
                                            City = item.locationsName,
                                            Region = item2.locationName,
                                            RLat = Convert.ToDouble(item2.lat),
                                            RLng = Convert.ToDouble(item2.lon),
                                            StartTime = Convert.ToDateTime(item4.startTime),
                                            EndTime = Convert.ToDateTime(item4.endTime),
                                            ModifyTime = DateTime.Now,
                                            UVI = uvi,
                                            Exposure = exposure
                                        };
                                        //insert into DB
                                        db.CWDUVI.Add(cwdData);
                                    }

                                }
                            }
                        }
                    }// end the foreach

                    if (uCity != null)
                        break;
                }//end foreach cItem

                db.SaveChanges();


                TimeSpan t2 = t1.Subtract(DateTime.Now);
                responseStr = "Update DB successfully! " + t2.TotalSeconds;

            }
            catch (Exception e) {
                responseStr =  e.ToString();
            }

                
            return responseStr;
        }

        public ActionResult EPA() {
            return View();
        }

        public async Task<string> PollAQI() {



            string cwbUrl = "https://opendata.epa.gov.tw/webapi/api/rest/datastore/355000000I-000259?sort=SiteName&offset=0&limit=1000";
            HttpClient client = new HttpClient();
            string result = await client.GetStringAsync(cwbUrl);
            List<ShareTravel.Models.AQIRecord> aqiRecord = JsonConvert.DeserializeObject<ShareTravel.Models.AQI>(result).result.records;

            var epaCityRegion = db.EPACityRegionLocation.ToArray();



            var queryAQI = from s in aqiRecord
                           join o in epaCityRegion on
                           new { Field1 = s.County, Field2 =  s.SiteName } equals new {  Field1 = o.City , Field2=  o.Region } into details
                           from o in details
                           select new EPAAQI {
                               City = s.County,
                               Region = s.SiteName,
                               RLat = o.RLat,
                               RLng = o.RLng,
                               AQI = s.AQI,
                               Status = s.Status,
                               Pollutant = s.Pollutant,
                               SO2 = s.SO2,
                               CO = s.CO,
                               CO_8hr = s.CO_8hr,
                               O3 = s.O3,
                               O3_8hr = s.O3_8hr,
                               PM10 = s.PM10,
                               PM25 = s.PM25,
                               NO2 = s.NO2,
                               NOx = s.NOx,
                               NO = s.NO,
                               WindSpeed = s.WindSpeed,
                               WindDirec = s.WindDirec,
                               PublishTime = Convert.ToDateTime(s.PublishTime),
                               PM25_AVG = s.PM25_AVG,
                               PM10_AVG = s.PM10_AVG                             
                           };

            if (queryAQI.Count() > 0) {
                db.Database.ExecuteSqlCommand("DELETE FROM EPAAQI");
                foreach (var item in queryAQI) {
                    db.EPAAQI.Add(item);
                }
                db.SaveChanges();
            }

            string json = JsonConvert.SerializeObject(queryAQI);
            return json;
        }

        public ActionResult Test()
        {
            return View();
        }
    }
}