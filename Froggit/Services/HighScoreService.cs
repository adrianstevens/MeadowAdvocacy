using HighScoreTracker.Models;
using SimpleJsonSerializer;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Froggit.Services
{
    public class HighScoreService
    {
        public static string ScoreUrl { get; set; } = "http://192.168.1.96:5265";
        public static string ApiEndpoint { get; set; } = "/api/FastestTimes";

        public static async Task PostTime(string name, int time, int deaths)
        {
            try
            {
                Console.WriteLine("PostTime ... ");
                var score = new FastestTime()
                {
                    PlayerName = name,
                    Time = time,
                    Deaths = deaths
                };

                var serializer = new JsonSerializer();

                Console.WriteLine($"Serialize {score}");
                string json = serializer.Serialize(score);

                Console.WriteLine("Create an HttpClient instance");
                using HttpClient httpClient = new HttpClient();

                Console.WriteLine("Set the base address for the API");
                httpClient.BaseAddress = new Uri(ScoreUrl);

                Console.WriteLine("Create the HttpContent for the JSON payload");
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine("Send the POST request to the API");
                HttpResponseMessage response = await httpClient.PostAsync(ApiEndpoint, content);

                Console.WriteLine("Check the response status");
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Data sent successfully!");
                }
                else
                {
                    Console.WriteLine("Error sending data: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}