using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DevTestCore
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //REST API: https://6165a7fccb73ea0017642166.mockapi.io/api/v1/
            // endpoint: airlines -> methods: GET
            // endpoint: passengers -> methods: GET / POST

            //TODO
            // Print airline with the most passengers

            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("https://6165a7fccb73ea0017642166.mockapi.io/api/v1/passengers");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            HttpResponseMessage airlinesResponse = await client.GetAsync("https://6165a7fccb73ea0017642166.mockapi.io/api/v1/airlines");
            airlinesResponse.EnsureSuccessStatusCode();
            string airresponseBody = await airlinesResponse.Content.ReadAsStringAsync();

            var Passengers = JsonConvert.DeserializeObject<List<Passenger>>(responseBody);
            var Airlines = JsonConvert.DeserializeObject<List<Airline>>(airresponseBody);

            int biggestAirlineId = Passengers.GroupBy(p => p.airlineId).OrderByDescending(g => g.Count()).First().Key;
            string biggestAirlineName = Airlines.Find(a => a.id == biggestAirlineId).name;
            Console.WriteLine("The airline with the most passengers is" + biggestAirlineName);

            // Print top 3 airlines by most number of unflown passengers and the list the unflown passengers for each of these airlines sorted by flight date
            var unflownPassengerAirlines = Passengers.Where(p => p.flightDate > DateTime.Now).GroupBy(p => p.airlineId).OrderByDescending(g => g.Count()).Take(3);

            foreach (var unflownAirline in unflownPassengerAirlines)
            {
                Console.WriteLine("Airline: " + Airlines.Where(a => a.id == unflownAirline.Key).FirstOrDefault().name);
                unflownAirline.OrderBy(p => p.flightDate);
                foreach (var passenger in unflownAirline)
                {
                    Console.WriteLine("Passenger: " + passenger.name + " | FlightDate :" + passenger.flightDate.ToString("dd/MM/yyyy"));
                }
            }

            // Create new passenger for the airline with most passengers
            Passenger newPassenger = new Passenger(75, "Achraf Bouanani", biggestAirlineId, DateTime.Today.AddDays(1));
            string request = JsonConvert.SerializeObject(newPassenger);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://6165a7fccb73ea0017642166.mockapi.io/api/v1/passengers");
            httpWebRequest.ContentType = "application/json";
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] josnByte = encoding.GetBytes(request);
            httpWebRequest.ContentLength = josnByte.Length;
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(request);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string jsonResponse = streamReader.ReadToEnd();
            }

        }

        public class Passenger
        {
            public int id { get; set; }
            public string name { get; set; }
            public int airlineId { get; set; }
            public DateTime flightDate { get; set; }

            public Passenger(int id, string name, int airlineId, DateTime flightDate)
            {
                this.id = id;
                this.name = name;
                this.airlineId = airlineId;
                this.flightDate = flightDate;
            }
        }

        public class Airline
        {
            public int id { get; set; }
            public string name { get; set; }
            public string country { get; set; }
            public string logo { get; set; }
            public string slogan { get; set; }
            public string head_quaters { get; set; }
            public string website { get; set; }
            public string established { get; set; }
        }
    }
}
