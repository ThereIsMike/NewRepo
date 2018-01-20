using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using taskitnowService.Helpers;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TChek
{
    internal class Program
    {
        private static IDisposable Cancel;
        private static int CountNewItems = 0;

        private static void Main(string[] args)
        {
            Console.WriteLine("Message sending demo");
            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                if (Console.ReadKey().Key != ConsoleKey.S)
                {
                    CountNewItems++;
                    if (Cancel != null)
                    {
                        Cancel.Dispose();
                        Cancel = null;
                    }
                    Cancel = Observable
                     .Timer(TimeSpan.FromSeconds(10))
                     .Subscribe(
                     x =>
                     {
                         Console.WriteLine("Clicked: " + CountNewItems + " times");
                         SendMessage(CountNewItems.ToString());
                         CountNewItems = 0;
                     });
                }
            }
        }

        private static void SendMessage(string item)
        {
            var data = new PushRequest();
            data.Text = item.ToString();

            var url = Settings.API_URL;

            using (var client = new HttpClient())
            {
                Task.WaitAll(client.PostAsync(url,
                    new StringContent(JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8, "application/json"))
                        .ContinueWith(response =>
                        {
                            Console.WriteLine(response.Status);
                            Console.WriteLine("Message sent: check the client device notification tray.");
                        }));
            }
        }

        public class PushRequest
        {
            [JsonProperty("action")]
            public string Action { get; set; }

            [JsonProperty("silent")]
            public bool Silent { get; set; }

            [JsonProperty("tags")]
            public string[] Tags { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }
        }
    }
}