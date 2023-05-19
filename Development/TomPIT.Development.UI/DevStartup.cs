using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Ide;
using TomPIT.Reflection;
using TomPIT.Startup;

namespace TomPIT.Development
{
    public class DevStartup : IStartupClient
    {
        public void Initialize(IStartupHost instance)
        {
            instance.Booting += OnBooting;
            instance.ConfiguringServices += OnConfiguringServices;
            instance.ConfiguringRouting += OnConfiguringRouting;
            instance.ConfigureEmbeddedStaticResources += OnConfigureEmbeddedStaticResources;
        }

        private void OnConfigureEmbeddedStaticResources(object sender, System.Collections.Generic.List<System.Reflection.Assembly> e)
        {
            e.Add(typeof(DevStartup).Assembly);
        }

        private void OnConfiguringRouting(object sender, IEndpointRouteBuilder e)
        {
            RegisterDesignersRouting(e);

            Configuration.Routing.Register(e);
            IdeRouting.Register(e, "Ide", "ide/{microservice}");
        }

        private void OnBooting(object sender, System.EventArgs e)
        {
            Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
        }

        private void OnConfiguringServices(object sender, IServiceCollection e)
        {
            //e.AddHostedService<AutoFixRunner>();
            //e.AddHostedService<ComponentAnalysisRunner>();
        }

        private void OnTenantInitialize(object sender, TenantArgs e)
        {
            //e.Tenant.RegisterService(typeof(IAutoFixService), typeof(AutoFixService));
            DevelopmentBootstrapper.Initialize(sender, e);
            IdeBootstrapper.Initialize(sender, e);
        }

        private void RegisterDesignersRouting(IEndpointRouteBuilder builder)
        {
            foreach (var i in Tenant.GetService<IDesignService>().QueryDesigners())
            {
                var t = TypeExtensions.GetType(i);

                if (t is null)
                    continue;

                var template = t.CreateInstance<IMicroServiceTemplate>();

                template.RegisterRoutes(builder);
            }
        }
    }
}
