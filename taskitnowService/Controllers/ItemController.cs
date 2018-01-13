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

namespace taskitnowService.Controllers
{
    public class ItemController : TableController<Item>
    {
        private IDisposable Cancel;
        private int CountNewItems = 0;

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
            this.CountNewItems++;
            if (this.Cancel != null)
            {
                this.Cancel.Dispose();
                this.Cancel = null;
            }

            this.Cancel = Observable
             .Timer(TimeSpan.FromSeconds(60))
             .Subscribe(
             async x =>
             {
                 await SendTemplateNotificationAsync(string.Format("Items added")).ConfigureAwait(false);
                 this.CountNewItems = 0;
             });

            Item current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            taskitnowContext context = new taskitnowContext();
            this.DomainManager = new EntityDomainManager<Item>(context, this.Request);
        }

        private async Task SendTemplateNotificationAsync(string item)
        {
            NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString(Settings.HUB_KEY, Settings.HUB_NAME);
            Dictionary<string, string> templateParams = new Dictionary<string, string>();
            templateParams["messageParam"] = item;
            await hub.SendTemplateNotificationAsync(templateParams, "");
        }
    }
}