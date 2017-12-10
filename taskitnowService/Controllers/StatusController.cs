using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using taskitnowService.DataObjects;
using taskitnowService.Models;

namespace taskitnowService.Controllers
{
    public class StatusController : TableController<Status>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            taskitnowContext context = new taskitnowContext();
            DomainManager = new EntityDomainManager<Status>(context, Request);
        }

        // GET tables/TodoItem
        public IQueryable<Status> GetAllStatus()
        {
            return Query();
        }

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Status> GetStatus(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Status> PatchStatus(string id, Delta<Status> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostStatus(Status item)
        {
            Status current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteStatus(string id)
        {
            return DeleteAsync(id);
        }
    }
}