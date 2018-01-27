using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web;
using ToShare;

namespace taskitnowService.Helpers
{
    public sealed class Statistics
    {
        public IObservable<long> Watch = Observable.Timer(TimeSpan.FromSeconds(60));
        private static readonly Statistics instance = new Statistics();

        private int recentlyAddedItems = 0;

        private Statistics()
        {
        }

        public static Statistics Instance
        {
            get
            {
                return instance;
            }
        }

        public IDisposable Cancel { get; set; }

        public IObservable<long> Check { get; set; } = Observable.Timer(TimeSpan.FromSeconds(60));

        public bool ReadyToSend { get; set; }

        public int ClearItemCounter()
        {
            this.recentlyAddedItems = 0;
            return this.recentlyAddedItems;
        }

        public int IncrementItemCounter()
        {
            if (this.Cancel != null)
            {
                this.Cancel.Dispose();
                this.Cancel = null;
            }

            this.Cancel = Observable
             .Timer(TimeSpan.FromSeconds(60))
             .Subscribe(
              x =>
              {
                  SendMessage($"{this.recentlyAddedItems} items added");
                  this.ClearItemCounter();
              });

            return this.recentlyAddedItems++;
        }

        private bool SendMessage(string item)
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
                            return response.Result.IsSuccessStatusCode;
                        }));
                return false;
            }
        }
    }
}