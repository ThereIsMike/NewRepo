using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(taskitnowService.Startup))]

namespace taskitnowService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}