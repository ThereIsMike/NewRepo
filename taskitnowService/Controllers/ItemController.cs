using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.NotificationHubs;
using taskitnowService.DataObjects;
using taskitnowService.Helpers;
using taskitnowService.Models;
using System.Reactive.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using ToShare;
using Newtonsoft.Json;
using System.Net;

namespace taskitnowService.Controllers
{
    public class ItemController : TableController<Item>
    {
        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteItem(string id)
        {
            return DeleteAsync(id);
        }

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Item> GetItem(string id)
        {
            return Lookup(id);
        }

        // GET tables/TodoItem
        public IQueryable<Item> GetItems()
        {
            return Query();
        }

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Item> PatchItem(string id, Delta<Item> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostItem(Item item)
        {
            Statistics.Instance.IncrementItemCounter();

            //var current = await InsertAsync(item);
            var current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            var context = new taskitnowContext();
            this.DomainManager = new EntityDomainManager<Item>(context, this.Request);
        }

        /// <summary>
        /// Sends the message. Work in the background and foreground.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Sends the template notification asynchronous. They work only in the foreground
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private async Task SendTemplateNotificationAsync(string item)
        {
            var hub = NotificationHubClient.CreateClientFromConnectionString(Settings.HUB_ENDPOINT, Settings.HUB_NAME);
            var templateParams = new Dictionary<string, string>();
            templateParams["messageParam"] = item;
            await hub.SendTemplateNotificationAsync(templateParams, "");
        }
    }
}