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
    public class RegisterController : ApiController
    {
        private NotificationHubClient hub;

        private NotificationHubClient Hub => this.hub ?? (this.hub = Helpers.Notifications.GetHub(this.Configuration.GetMobileAppSettingsProvider().GetMobileAppSettings()));

        // DELETE api/register/5
        [HttpDelete]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            try
            {
                await this.Hub.DeleteInstallationAsync(id);
                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PNS Error]: Issue deleting device installation {ex.Message}");
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        // PUT api/register
        [HttpPut]
        public async Task<HttpResponseMessage> Put([FromBody]DeviceInstallation deviceUpdate)
        {
            System.Diagnostics.Debug.WriteLine("Mobile App Registering");

            if (deviceUpdate == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            Dictionary<string, InstallationTemplate> templates = new Dictionary<string, InstallationTemplate>();
            foreach (var t in deviceUpdate.Templates)
            {
                templates.Add(t.Key, new InstallationTemplate { Body = t.Value.Body });
            }

            Installation installation = new Installation()
            {
                InstallationId = deviceUpdate.InstallationId,
                PushChannel = deviceUpdate.PushChannel,
                Tags = deviceUpdate.Tags,
                Templates = templates
            };

            switch (deviceUpdate.Platform)
            {
                case "apns":
                    installation.Platform = NotificationPlatform.Apns;
                    break;

                case "gcm":
                    installation.Platform = NotificationPlatform.Gcm;
                    break;

                default:
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            // In the backend we can control if a user is allowed to add tags
            installation.Tags = new List<string>(deviceUpdate.Tags);

            try
            {
                await this.Hub.CreateOrUpdateInstallationAsync(installation);
                return this.Request.CreateResponse(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PNS Error]: Issue registering device {ex.Message}");
                return this.Request.CreateResponse(false);
            }
        }
    }
}