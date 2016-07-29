using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Bot_Application1
{
    public static class Weather
    {
        public static async Task<string> GetCurrentWeather(string location, string feature)
        {
            using (var client = new HttpClient())
            {
                var escapedLocation = Regex.Replace(location, @"\W+", "_");

                dynamic response = JObject.Parse(await client.GetStringAsync($"http://api.wunderground.com/api/f7d8bb8056fd0664/{feature}/q/{escapedLocation}.json"));

                switch (feature)
                {
                    case "conditions":
                        dynamic observation = response.current_observation;
                        dynamic results = response.response.results;

                        if (observation != null)
                        {
                            string displayLocation = observation.display_location?.full;
                            decimal tempC = observation.temp_c;
                            string weather = observation.weather;

                            return $"It is {weather} and {tempC} degrees in {displayLocation}.";
                        }
                        else if (results != null)
                        {
                            return $"There is more than one '{location}'. Can you be more specific?";
                        }
                        break;
                    case "forecast":
                        dynamic forecast = response.forecast;
                        results = response.response.results;

                        if (forecast != null)
                        {
                            string forecastText = forecast.txt_forecast?.forecastday[0]?.fcttext_metric;

                            return $"Forecast for today is {forecastText} in {location}.";
                        }
                        else if (results != null)
                        {
                            return $"There is more than one '{location}'. Can you be more specific?";
                        }
                        break;
                }
                return null;
            }

        }
    }
}