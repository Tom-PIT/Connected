using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Development.Analysis;
using TomPIT.Environment;
using TomPIT.Ide;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Development
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var e = new ServicesConfigurationArgs
            {
                Authentication = AuthenticationType.MultiTenant
            };

            Instance.Initialize(InstanceType.Development, services, e);
            Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
            DevelopmentBootstrapper.Run();
            IdeBootstrapper.Run();
            Instance.InitializeShellServices();

            services.AddHostedService<ToolsRunner>();
            services.AddHostedService<AutoFixRunner>();
            services.AddHostedService<ComponentAnalysisRunner>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Instance.Configure(app, env, (f) =>
                {
                    RegisterDesignersRouting(f.Builder);

                    Configuration.Routing.Register(f.Builder);
                    IdeRouting.Register(f.Builder, "Ide", "ide/{microservice}");
                }
            );

            Instance.Run(app, env);
        }

        private void OnTenantInitialize(object sender, TenantArgs e)
        {
            e.Tenant.RegisterService(typeof(IAutoFixService), typeof(AutoFixService));
        }

        private void RegisterDesignersRouting(IEndpointRouteBuilder builder)
        {
            foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
            {
                var t = TypeExtensions.GetType(i);

                if (t == null)
                    continue;

                var template = t.CreateInstance<IMicroServiceTemplate>();

                template.RegisterRoutes(builder);
            }
        }
    }
}
