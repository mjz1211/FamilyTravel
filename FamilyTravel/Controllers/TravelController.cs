using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShareTravel.Models;
using Newtonsoft.Json;

namespace ShareTravel.Controllers
{
    public class TravelController : Controller
    {
        public ActionResult ListPlace(string lat, string lng)
        {
            ViewBag.Lat = lat;
            ViewBag.Lng = lng;
            ViewBag.WeatherDesc = db.CWDWeatherDescription.ToArray();
            ViewBag.UVI = db.CWDUVI.ToArray();
            return View();
        }

        public ActionResult Pano(string lat, string lng, string place_id, string name, string address, string image, string distance, string rating, string ptype)
        {
            //25.037953, 121.517455
            if (lat == null || lng == null)
            {
                lat = "25.037953";
                lng = "121.517455";
            }

            ViewBag.Lat = Convert.ToDouble(lat);
            ViewBag.Lng = Convert.ToDouble(lng);
            ViewBag.Place_Id = place_id;
            ViewBag.Name = name;
            ViewBag.Address = address;
            ViewBag.Image = image;
            ViewBag.Distance = distance;
            ViewBag.Rating = rating;
            ViewBag.PType = ptype;
            return View();
        }

        public ActionResult Add(string place_id, string image, string name, string address, string rating, string distance, string lat, string lng, string ptype)
        {
            Place place = new Place
            {
                Place_Id = place_id,
                Image = image,
                Name = name,
                Rating = Convert.ToDouble(rating),
                Distance = Convert.ToDouble(distance),
                Address = address,
                Lat = lat,
                Lng = lng,
                PType = ptype
            };

            if (Session["cart"] == null)
            {
                List<Place> placeList = new List<Place>();

                placeList.Add(place);
                Session["cart"] = placeList;
                

            }
            else
            {
                List<Place> placeList = (List<Place>)Session["cart"];
                placeList.Add(place);
            }

            return RedirectToAction("ListPlace", "Travel", new { lat = lat, lng = lng });
        }

        public ActionResult Cart(string command)
        {
            if (command == "clear") {
                List<Place> placeList = new List<Place>();

                Session["cart"] = placeList;
            }

            return View((List<Place>)Session["cart"]);
        }

        

        openTCLEntities db = new openTCLEntities();
        public ActionResult CreatePackage(PackageFormViewModel[] packageForm, string outputDate)
        {
            var stPIdList = db.ShareTravelPlace.Select(p => p.Place_Id).ToList();
            string placesField = "", packageName = "", memo = "",url_tag ="";

            List<ShareTravelPlace> stPlaceList = new List<ShareTravelPlace>();

            //List<PackageFormViewModel> pfList = new List<PackageFormViewModel>();
            for (int i = 0; i < packageForm.Length; i++)
            {
                //分為 package , place 存放
                //package:
                if (i == packageForm.Length - 1)
                {
                    placesField += packageForm[i].Place_Id;
                    packageName = packageForm[i].PackageName;
                    memo = packageForm[i].PackageMemo;
                    if (packageForm[i].URL_TAG != null) {
                        url_tag = packageForm[i].URL_TAG;
                    }
                }
                else
                    placesField += packageForm[i].Place_Id + ";";

                // DB 還沒有資料 才存
                //place:
                if (!stPIdList.Contains(packageForm[i].Place_Id))
                {
                    stPlaceList.Add(new ShareTravelPlace
                    {
                        Place_Id = packageForm[i].Place_Id,
                        PlaceName = packageForm[i].PlaceName,
                        Address = packageForm[i].Address,
                        Image = packageForm[i].Image,
                        Rating = Convert.ToDouble(packageForm[i].Rating),
                        Lat = Convert.ToDouble(packageForm[i].Lat),
                        Lng = Convert.ToDouble(packageForm[i].Lng),
                        PType = packageForm[i].PType
                        
                       
                    });
                }

            }

            foreach (var item in stPlaceList)
            {
                db.ShareTravelPlace.Add(item);
            }

            var stpgArr = db.ShareTravelPackage.ToArray();

            var pgResult = from p in stpgArr
                           where p.PackageName == packageName
                           select p.STP_Id;

            foreach (var item in pgResult) {
                ShareTravelPackage stpg = db.ShareTravelPackage.Find(item);
                db.ShareTravelPackage.Remove(stpg);
            }

            db.ShareTravelPackage.Add(new ShareTravelPackage
            {
                PackageName = packageName,
                Memo = memo,
                TravelDateTime = Convert.ToDateTime(outputDate),//DateTime.Now,
                Places = placesField
            });


            db.SaveChanges();

            if (url_tag == "BP") {
                return RedirectToAction("ListBackupPackage", "Travel"); 
            }

            //TempData["PFList"] = pfList;

            return RedirectToAction("ListPackage", "Travel"); // 先將packageForm 丟到 ViewBag.package 然後 redirect 到 Package(); 未來是要 redirect 到 ListPackage();
        }



        public ActionResult ListComparePackage()
        {
            return View(db.ShareTravelPackage.ToArray());
        }

        public ActionResult ListPackage()
        {

            ViewBag.STPlaces = db.ShareTravelPlace.ToArray();
            return View(db.ShareTravelPackage.ToArray());
        }


        public ActionResult ListBackupPackage()
        {

            List<ShareTravelPackage> stpList = new List<ShareTravelPackage>();
            ViewBag.STPlaces = db.ShareTravelPlace.ToArray();

            var stpArr = db.ShareTravelPackage.ToArray();

            var backupResult = from p in stpArr
                               where p.PackageName.IndexOf("備案") > -1
                               select p;
            foreach (var item in backupResult) {
                string packageName = item.PackageName.Substring(0, item.PackageName.IndexOf("備案"));
                var packageResult = from p in stpArr
                                    where p.PackageName == packageName
                                    select p;
                stpList.Add(packageResult.First());
                stpList.Add(item);
            }

            return View(stpList.ToArray());
        }


        public ActionResult Package(string packageName) // 用 packageName 當 key
        {

            if (packageName != null)
            {

                string uvi = "", uviNote ="",aqi = "", aqiNote ="";

                var epaAQIArr = db.EPAAQI.ToArray();

                var feedingArr = db.AllFeedingRoom.ToArray();

                var result = from p in db.ShareTravelPackage.ToArray()
                             where p.PackageName == packageName
                             select p;

                List<PackageFormViewModel> pfList = new List<PackageFormViewModel>();

                //todo 帶出 place string 並 split , 把每一個 place 轉成 packageFormViewModel
                string pName = "", pMemo = "", pDate = "";
                foreach (var item in result)
                {
                    string[] places = item.Places.Split(';');
                    pName = item.PackageName;
                    pMemo = item.Memo;
                    pDate = item.TravelDateTime.ToString("yyyy/MM/dd");
                    foreach (var pItem in places)
                    {
                        var pInfo = from s in db.ShareTravelPlace
                                    where s.Place_Id == pItem
                                    select s;

                        var stPlace = pInfo.First();

                        //query place's location, find the lowest distance 
                        var cityRegion = db.CWBCityRegionLocation.ToArray();

                        var queryLocation = from s in cityRegion
                                            select new
                                            {
                                                Distance = GeolocationUtils.GetDistance(s.RLat, s.RLng, stPlace.Lat, stPlace.Lng),
                                                City = s.City,
                                                Region = s.Region
                                            };

                        queryLocation = queryLocation.OrderBy(c => c.Distance);

                        string city = queryLocation.First().City;
                        string region = queryLocation.First().Region;

                        var queryWeather = from s in db.CWDWeatherDescription.ToArray()
                                           where s.StartTime.ToString("yyyy/MM/dd") == pDate && s.City == city && s.Region == region
                                           select s;

                        string weather = "", rain = "", temp = "", status = "", wind = "", humidity = "",temp2="";

                        if (queryWeather.Count() > 0) {
                            weather = queryWeather.First().Weather;
                            rain = queryWeather.First().Rain;
                            if (rain == null || rain == "" || rain.IndexOf("降雨機率0%") > -1)
                            {
                                rain = "0";
                            }
                            else {
                                rain = rain.Substring(4, 2);
                                
                            }

                            temp = queryWeather.First().Temp; //溫度攝氏28至32度
                            status = queryWeather.First().Status;  //舒適至悶熱
                            wind = queryWeather.First().Wind;    
                            humidity = queryWeather.First().Humidity.Trim(); // 相對濕度為71%

                            temp2 = temp.Substring(7, 2);
                            temp = temp.Substring(4, 2);
                            humidity = humidity.Substring(5, 2);
                            // 
                        }

                        var queryUVI = (from s in db.CWDUVI.ToArray()
                                        where s.StartTime.ToString("yyyy/MM/dd") == pDate && s.City == city && s.Region == region
                                        select s);

                        uvi = queryUVI.First().UVI.ToString();
                        uviNote = queryUVI.First().Exposure;
                        //AQI 先用同縣市第一筆
                        var queryAQI = from s in epaAQIArr
                                       where s.City == city
                                       select s;

                        aqi = queryAQI.First().AQI;
                        aqiNote = "(" + queryAQI.First().Status + ")";


                        //query feeding
                        var queryFeeding = from s in feedingArr                                                                                   
                                            select new MyFeedingRoom
                                            {
                                                Distance = Math.Round(GeolocationUtils.GetDistance(s.lat, s.lng, stPlace.Lat, stPlace.Lng)*100)/100,
                                                Name = s.name,
                                                Address = s.address,
                                                Tel = s.tel,
                                                Lat = s.lat,
                                                Lng = s.lng,
                                                Opentime = s.opentime
                                            };

                        queryFeeding = queryFeeding.OrderBy(c => c.Distance).Take(5);

                        
                        pfList.Add(new PackageFormViewModel
                        {
                            PlaceName = stPlace.PlaceName,
                            Address = stPlace.Address,
                            Image = stPlace.Image,
                            Rating = Convert.ToString(stPlace.Rating),
                            Place_Id = stPlace.Place_Id,
                            Lat = Convert.ToString(stPlace.Lat),
                            Lng = Convert.ToString(stPlace.Lng),
                            PackageName = pName,
                            PackageMemo = pMemo,
                            Date = pDate,
                            Weather = weather,
                            Rain = rain,
                            Temp = (temp + " - " + temp2),
                            Status = status,
                            Wind = wind,
                            Humidity = humidity,
                            City = city,
                            Region = region,
                            UVI = uvi,
                            UVINote = uviNote,
                            AQI = aqi,
                            AQINote = aqiNote,
                            JsonStrFeedingRooms = JsonConvert.SerializeObject(queryFeeding)
                        });




                    }
                }
                
                // note the truncate error

                ViewBag.PFList = pfList;

                string maxRain = pfList.Max(c => c.Rain);
                ViewBag.Rain = maxRain;

                string minTemp = pfList.Min(c => c.Temp.Substring(0, 2));

                ViewBag.Temp = minTemp + " - " + pfList.Max(c => c.Temp.Substring(c.Temp.IndexOf("-")+2,2));

                string minHumidity = pfList.Min(c => c.Humidity);
                string maxHumidity = pfList.Max(c => c.Humidity);

                
                ViewBag.Humidity =  (minHumidity == maxHumidity)? maxHumidity : minHumidity + " - " + maxHumidity;

                string maxUVI = pfList.Max(c => c.UVI);
                ViewBag.UVI = maxUVI;

                string minAQI = pfList.Min(c => c.AQI);
                string maxAQI = pfList.Max(c => c.AQI);
                ViewBag.AQI = (minAQI == maxAQI) ? minAQI : minAQI + " - " + maxAQI;


                ViewBag.Span1Icon = "<img src=\"https://familytravel.azurewebsites.net/Content/images/happy.png\" />";
                ViewBag.Span2Icon = "<img src=\"https://familytravel.azurewebsites.net/Content/images/happy.png\" />";
                ViewBag.Span3Icon = "<img src=\"https://familytravel.azurewebsites.net/Content/images/happy.png\" />";
                ViewBag.Span4Icon = "<img src=\"https://familytravel.azurewebsites.net/Content/images/happy.png\" />";
                ViewBag.Span5Icon = "<img src=\"https://familytravel.azurewebsites.net/Content/images/happy.png\" />";
                ViewBag.Span1Note = "良好";
                ViewBag.Span2Note = "良好";
                ViewBag.Span3Note = "良好";
                ViewBag.Span4Note = "良好";
                ViewBag.Span5Note = "良好";


                if (Convert.ToInt16(maxRain) > 0)
                {
                    ViewBag.Span1Icon = "<img src=\"https://familytravel.azurewebsites.net/Content/images/umbrella.png\" />";
                    ViewBag.Span1Note = "攜帶雨具";
                }

                if (Convert.ToInt16(minTemp) > 25)
                {
                    ViewBag.Span2Icon = "<img src=\"https://familytravel.azurewebsites.net/Content/images/water.png\" />";
                    ViewBag.Span2Note = "喝水預防中暑";
                }

                if (Convert.ToInt16(minHumidity) >= 50) {
                    ViewBag.Span3Icon = "<img src=\"https://familytravel.azurewebsites.net/Content/images/pimples.png\" />";
                    ViewBag.Span3Note = "預防過敏";
                }


                if (Convert.ToInt16(maxUVI) >= 8)
                {
                    ViewBag.Span4Icon = "<img src=\"https://familytravel.azurewebsites.net/Content/images/hat-and-glasses.png\" />";
                    ViewBag.Span4Note = "注意防曬";
                }

                if (Convert.ToInt16(aqi) > 100)
                {
                    ViewBag.Span5Icon = "<img src=\"https://familytravel.azurewebsites.net/Content/images/mask.png\" />";
                    ViewBag.Span5Note = "注意空污";
                }

            }
            //ViewBag.PFList = TempData["PFList"];
            return View();
        }

        public ActionResult PackageSlide(string packageName, string slideTheme)
        {

            ViewBag.Theme = slideTheme;
            List<Map> mapList = new List<Map>();
            List<Place> places = new List<Place>();
            string pName = "", pMemo = "", pDate = "";
            var cwdUVIArr = db.CWDUVI.ToArray();
            var epaAQIArr = db.EPAAQI.ToArray();

            if (packageName != null)
            {
                var result = from p in db.ShareTravelPackage.ToArray()
                             where p.PackageName == packageName
                             select p;


                //todo 帶出 place string 並 split , 把每一個 place 轉成 packageFormViewModel

                foreach (var item in result)
                {
                    string[] placeStrs = item.Places.Split(';');
                    pName = item.PackageName;
                    pMemo = item.Memo;
                    pDate = item.TravelDateTime.ToString("yyyy/MM/dd");
                    foreach (var pItem in placeStrs)
                    {
                        var pInfo = from s in db.ShareTravelPlace
                                    where s.Place_Id == pItem
                                    select s;

                        var stPlace = pInfo.First();

                        //query place's location, find the lowest distance 
                        var cityRegion = db.CWBCityRegionLocation.ToArray();

                        var queryLocation = from s in cityRegion
                                            select new
                                            {
                                                Distance = GeolocationUtils.GetDistance(s.RLat, s.RLng, stPlace.Lat, stPlace.Lng),
                                                City = s.City,
                                                Region = s.Region
                                            };

                        queryLocation = queryLocation.OrderBy(c => c.Distance);

                        string city = queryLocation.First().City;
                        string region = queryLocation.First().Region;

                        var queryWeather = from s in db.CWDWeatherDescription.ToArray()
                                           where s.StartTime.ToString("yyyy/MM/dd") == pDate && s.City == city && s.Region == region
                                           select s;

                        string weather = "", rain = "", temp = "", status = "", wind = "", humidity = "", temp2 = "";

                        if (queryWeather.Count() > 0)
                        {
                            weather = queryWeather.First().Weather;

                            rain = queryWeather.First().Rain;
                            if (rain == null || rain == "" || rain.IndexOf("降雨機率0%") > -1)
                            {
                                rain = "0";
                            }
                            else
                            {
                                rain = rain.Substring(4, 2);

                            }

                            temp = queryWeather.First().Temp; //溫度攝氏28至32度
                            status = queryWeather.First().Status;  //舒適至悶熱
                            wind = queryWeather.First().Wind;
                            humidity = queryWeather.First().Humidity.Trim(); // 相對濕度為71%

                            temp2 = temp.Substring(7, 2);
                            temp = temp.Substring(4, 2);
                            humidity = humidity.Substring(5, 2);
                            // 
                        }


                        var queryUVI = (from s in cwdUVIArr
                                        where s.StartTime.ToString("yyyy/MM/dd") == pDate && s.City == city && s.Region == region
                                        select s);

                        string uvi = queryUVI.First().UVI.ToString() + "(" + queryUVI.First().Exposure + ")";

                        //AQI 先用同縣市第一筆
                        var queryAQI = from s in epaAQIArr
                                       where s.City == city
                                       select s;

                        string aqi = queryAQI.First().AQI + "(" + queryAQI.First().Status + ")";


                        places.Add(new Place
                        {
                            Name = stPlace.PlaceName,
                            Address = stPlace.Address,
                            Image = stPlace.Image,
                            Rating = stPlace.Rating,
                            Place_Id = stPlace.Place_Id,
                            Lat = Convert.ToString(stPlace.Lat),
                            Lng = Convert.ToString(stPlace.Lng),
                            Weather = weather,
                            Rain = rain,
                            Temp = (temp + " - " + temp2),
                            Status = status,
                            Wind = wind,
                            Humidity = humidity,
                            UVI = uvi,
                            AQI = aqi
                        });



                    }
                }

            }


            string pathString = "", title = "";
            Place[] pArray = places.ToArray();
            for (int i = 0; i < pArray.Length; i++)
            {

                if (i == pArray.Length - 1)
                {
                    break; //只有點與點之間要畫 map , 最後一個點不用
                }
                title = pArray[i].Name + " to " + pArray[i + 1].Name;
                pathString = pArray[i].Lat + "," + pArray[i].Lng + "|" + pArray[i + 1].Lat + "," + pArray[i + 1].Lng;
                mapList.Add(new Map { Title = title, Path = pathString });

            }

            ViewBag.Places = places;
            ViewBag.Maps = mapList.ToArray();
            ViewBag.PName = pName;
            ViewBag.PMemo = pMemo;
            ViewBag.PDate = pDate;

            return View();
        }


        public ActionResult ComparePackage(SelectedPackage[] sp)
        {
            if (sp != null) {
                var pNames = sp.Where(c => c.Selected).Select(s => s.PackageName);


                var result = from p in db.ShareTravelPackage.ToArray()
                             where pNames.ToArray().Contains(p.PackageName)
                             select p;

                //string jstr = "  { \"name\": \"abc\", \"age\": 50 },{ \"age\": \"25\", \"hobby\": \"swimming\" },{ \"name\": \"xyz\", \"hobby\": \"programming\" }";
                string jstr2 = "";

                var stPlaceArr = db.ShareTravelPlace.ToArray();

                int j = 0;
                string pDate = "";
                var wArr = db.CWDWeatherDescription.ToArray();
                var cityRegion = db.CWBCityRegionLocation.ToArray();

                foreach (var item in result)
                {

                    string[] placeIdArr = item.Places.Split(';');
                    /*
                    var places = from p in db.ShareTravelPlace.ToArray()
                                 where placeIdArr.Contains(p.Place_Id)
                                 select p;
                                 */
                    int i = 0, len = 0;
                    string jsStr = "", record = "";
                    pDate = item.TravelDateTime.ToString("yyyy/MM/dd");
                    jsStr = "\"行程名稱\":" + "\"" + pDate + item.PackageName + "\",";
                    
                    for (i = 0, len = placeIdArr.Length; i < len; i++)
                    {

                        int tempNo = 0;
                        if (i == len - 1)
                        {
                            var places = from p in stPlaceArr
                                         where p.Place_Id == placeIdArr[i]
                                         select p;

                            var pItem = places.First();

                            //query place's location, find the lowest distance 

                            var queryLocation = from s in cityRegion
                                                select new
                                                {
                                                    Distance = GeolocationUtils.GetDistance(s.RLat, s.RLng, pItem.Lat, pItem.Lng),
                                                    City = s.City,
                                                    Region = s.Region
                                                };

                            queryLocation = queryLocation.OrderBy(c => c.Distance);

                            string city = queryLocation.First().City;
                            string region = queryLocation.First().Region;

                            var queryWeather = from s in wArr
                                               where s.StartTime.ToString("yyyy/MM/dd") == pDate && s.City == city && s.Region == region
                                               select s;

                            string wIcon = GetIcon(queryWeather.First().Weather);


                            tempNo = i + 1;
                            jsStr += "\"景點" + tempNo + "\":" + "\"<button  class='btn btn-info' data-toggle='modal' data-target='#myModal' onclick='' image='" + pItem.Image + "' address='" + pItem.Address + "'  rating='" + pItem.Rating + "' placeName='" + pItem.PlaceName + "'><i class='" + wIcon +"' style='padding: 5px 10px 5px 10px;background:red;border-radius:5px;'></i>" + pItem.PlaceName + "</button>\"";

                        }
                        else
                        {

                            var places = from p in stPlaceArr
                                         where p.Place_Id == placeIdArr[i]
                                         select p;

                            var pItem = places.First();

                            //query place's location, find the lowest distance 

                            var queryLocation = from s in cityRegion
                                                select new
                                                {
                                                    Distance = GeolocationUtils.GetDistance(s.RLat, s.RLng, pItem.Lat, pItem.Lng),
                                                    City = s.City,
                                                    Region = s.Region
                                                };

                            queryLocation = queryLocation.OrderBy(c => c.Distance);

                            string city = queryLocation.First().City;
                            string region = queryLocation.First().Region;

                            var queryWeather = from s in wArr
                                               where s.StartTime.ToString("yyyy/MM/dd") == pDate && s.City == city && s.Region == region
                                               select s;

                            string wIcon = GetIcon(queryWeather.First().Weather);

                            var places2 = from p in stPlaceArr
                                          where p.Place_Id == placeIdArr[i + 1]
                                          select p;

                            var nextPItem = places2.First();

                            tempNo = i + 1;

                            jsStr += "\"景點" + tempNo + "\":" + "\"<button class='btn btn-info' data-toggle='modal' data-target='#myModal' onclick='' image='" + pItem.Image + "' address='" + pItem.Address + "'  rating='" + pItem.Rating + "' placeName='" + pItem.PlaceName + "'><i class='" + wIcon + "' style='padding: 5px 10px 5px 10px;background:red;border-radius:5px;'></i>" + pItem.PlaceName + "</button>\",\"距離" + tempNo + "\":\"<span id='spanDriving" + j + i + "' class='driving' onclick='' lat1='" + pItem.Lat + "' lng1='" + pItem.Lng + "' lat2='" + nextPItem.Lat + "' lng2='" + nextPItem.Lng + "'  currentPName='" + pItem.PlaceName + "' nextPName='" + nextPItem.PlaceName + "' ></span><span id='spanTransit" + j + i + "' class='transit' onclick='' lat1='" + pItem.Lat + "' lng1='" + pItem.Lng + "' lat2='" + nextPItem.Lat + "' lng2='" + nextPItem.Lng + "'  currentPName='" + pItem.PlaceName + "' nextPName='" + nextPItem.PlaceName + "' fare='' ></span>\",";
                        }



                    }

                    record = "{" + jsStr + "},";
                    jstr2 += record;
                    j++;
                }


                ViewBag.JSON = jstr2;

            }


            return View();
        }

        public string GetIcon(string w)
        {
            if (w == "多雲午後短暫雷陣雨" || w == "陰短暫陣雨或雷雨" || w == "多雲短暫陣雨或雷雨" || w == "陰時多雲短暫陣雨或雷雨" || w == "多雲時陰短暫陣雨")
            {
                return "wi wi-day-rain";
            }
            else if (w == "多雲" || w == "陰天")
            {
                return "wi wi-day-cloudy";
            }
            else if (w == "多雲時陰" || w == "陰時多雲")
            {
                return "wi wi-day-cloudy-high";
            }
            else if (w == "多雲時晴" || w == "時晴多雲" || w == "晴時多雲")
            {
                return "wi wi-day-sunny-overcast";
            }
            else if (w == "晴午後短暫雷陣雨")
            {
                return "wi wi-day-thunderstorm";
            }
            else
            {
                return "wi wi-day-cloudy";
            }
        }


        public ActionResult BackupPackage(string packageName)
        {
            string jsonStrWeather = "";
            if (packageName != null)
            {
                var result = from p in db.ShareTravelPackage.ToArray()
                             where p.PackageName == packageName
                             select p;

                List<PackageFormViewModel> pfList = new List<PackageFormViewModel>();

                //todo 帶出 place string 並 split , 把每一個 place 轉成 packageFormViewModel
                string pName = "", pMemo = "", pDate = "";
                DateTime travelDT = DateTime.Now;
                foreach (var item in result)
                {
                    string[] places = item.Places.Split(';');
                    pName = item.PackageName;
                    pMemo = item.Memo;
                    pDate = item.TravelDateTime.ToString("yyyy/MM/dd");
                    travelDT = item.TravelDateTime;
                    foreach (var pItem in places)
                    {
                        var pInfo = from s in db.ShareTravelPlace
                                    where s.Place_Id == pItem
                                    select s;
                        var stPlace = pInfo.First();

                        //query place's location, find the lowest distance 
                        var cityRegion = db.CWBCityRegionLocation.ToArray();

                        var queryLocation = from s in cityRegion
                                            select new
                                            {
                                                Distance = GeolocationUtils.GetDistance(s.RLat, s.RLng, stPlace.Lat, stPlace.Lng),
                                                City = s.City,
                                                Region = s.Region
                                            };

                        queryLocation = queryLocation.OrderBy(c => c.Distance);

                        string city = queryLocation.First().City;
                        string region = queryLocation.First().Region;

                        var queryWeather = from s in db.CWDWeatherDescription.ToArray()
                                           where s.StartTime.ToString("yyyy/MM/dd") == pDate && s.City == city && s.Region == region
                                           select s;

                        string weather = "", rain = "", temp = "", status = "", wind = "", humidity = "", temp2 = "";

                        if (queryWeather.Count() > 0)
                        {
                            weather = queryWeather.First().Weather;

                            rain = queryWeather.First().Rain;
                            if (rain == null || rain == "" || rain.IndexOf("降雨機率0%") > -1)
                            {
                                rain = "0";
                            }
                            else
                            {
                                rain = rain.Substring(4, 2);
                            }

                            temp = queryWeather.First().Temp; //溫度攝氏28至32度
                            status = queryWeather.First().Status;  //舒適至悶熱
                            wind = queryWeather.First().Wind;
                            humidity = queryWeather.First().Humidity.Trim(); // 相對濕度為71%

                            temp2 = temp.Substring(7, 2);
                            temp = temp.Substring(4, 2);
                            humidity = humidity.Substring(5, 2);
                            // 
                        }


                        pfList.Add(new PackageFormViewModel
                        {
                            PlaceName = stPlace.PlaceName,
                            Address = stPlace.Address,
                            Image = stPlace.Image,
                            Rating = Convert.ToString(stPlace.Rating),
                            Place_Id = stPlace.Place_Id,
                            Lat = Convert.ToString(stPlace.Lat),
                            Lng = Convert.ToString(stPlace.Lng),
                            PackageName = pName,
                            PackageMemo = pMemo,
                            PType = stPlace.PType,
                            Date = pDate,
                            Weather = weather,
                            Rain = rain,
                            Temp = (temp + " - " + temp2),
                            Status = status,
                            Wind = wind,
                            Humidity = humidity,
                            City = city,
                            Region = region
                        });


                    }
                }

                // prepare the pDate weather data, 可能有 06:00 或 18:00 開始的資料, 只抓白天的資料
                var queryDateWeather = from s in db.CWDWeatherDescription.ToArray()
                                       where s.StartTime.ToString("yyyy/MM/dd") == travelDT.ToString("yyyy/MM/dd") && s.StartTime.ToString("HH") == "06"
                                       select new {
                                           City = s.City,
                                           Region = s.Region,
                                           RLat = s.RLat,
                                           RLng = s.RLng,
                                           Weather = s.Weather,
                                           Rain = (s.Rain =="")?"0": s.Rain.Substring(4, 2),
                                           Temp = s.Temp.Substring(4, 2) + " - " + s.Temp.Substring(7, 2),
                                           Humidity = s.Humidity.Substring(6,2),
                                           Distance = ""
                                       };

                //
                jsonStrWeather = JsonConvert.SerializeObject(queryDateWeather);

                ViewBag.PFList = pfList;
                ViewBag.JsonStrWeather = jsonStrWeather;
            }
            return View();
        }

        public ActionResult DeletePackage(int stp_Id) {
            ShareTravelPackage stp = db.ShareTravelPackage.Find(stp_Id);

            if (stp != null) {

                var backupResult = from p in db.ShareTravelPackage.ToArray()
                                   where p.PackageName == stp.PackageName + "備案"
                                   select p;

                foreach (var item in backupResult)
                {
                    db.ShareTravelPackage.Remove(item);
                }


                db.ShareTravelPackage.Remove(stp);
                db.SaveChanges();
            }

            return RedirectToAction("ListPackage","Travel");
        }

        public ActionResult Lab() {
            return View();
        }


    }
}