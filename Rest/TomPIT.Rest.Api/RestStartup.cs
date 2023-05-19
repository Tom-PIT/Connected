using TomPIT.Rest.Routing;
using TomPIT.Startup;

namespace TomPIT.Rest
{
    public class RestStartup : IStartupClient
    {
        public void Initialize(IStartupHost host)
        {
            host.ConfiguringRouting += OnConfiguringRouting;
        }

        private void OnConfiguringRouting(object sender, Microsoft.AspNetCore.Routing.IEndpointRouteBuilder e)
        {
            RestRouting.Register(e);
        }
    }
}
