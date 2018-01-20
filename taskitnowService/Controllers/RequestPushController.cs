using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.NotificationHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ToShare;

namespace taskitnowService.Controllers
{
    [MobileAppController]
    public class RequestPushController : ApiController
    {
        private NotificationHubClient hub;

        private NotificationHubClient Hub => this.hub ?? (this.hub = Helpers.Notifications.GetHub(this.Configuration.GetMobileAppSettingsProvider().GetMobileAppSettings()));

        // POST api/requestpush
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody]PushRequest pushRequest)
        {
            if (pushRequest == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            Dictionary<string, string> templateParams = new Dictionary<string, string>();

            if (pushRequest.Silent)
            {
                templateParams["silentMessageParam"] = "1";
                templateParams["actionParam"] = pushRequest.Action;
            }
            else
            {
                templateParams["messageParam"] = pushRequest.Text;
            }

            try
            {
                // Send the push notification and log the results.

                // Send the push notification and log the results.
                var result = await this.Hub.SendTemplateNotificationAsync(templateParams, "");

                // Write the success result to the logs.
                System.Diagnostics.Trace.WriteLine($"Outcome: {result.State.ToString()}");

                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                // Write the failure result to the logs.
                System.Diagnostics.Trace.WriteLine($"Push.SendAsync Error: {ex.Message}");

                return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}