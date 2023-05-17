using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Ide;
using TomPIT.Management.Deployment;
using TomPIT.Runtime;

namespace TomPIT.Management
{
   public class Startup
   {
      public void ConfigureServices(IServiceCollection services)
      {
         var e = new ServicesConfigurationArgs
         {
            Authentication = AuthenticationType.MultiTenant
         };

         //Instance.Initialize(services, e);

         IdeBootstrapper.Run();
         ManagementBootstrapper.Run();

         services.AddHostedService<InstallerService>();
         services.AddHostedService<UpdateService>();
         services.AddHostedService<PublishService>();
      }

      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         Instance.Configure(app, env, (f) =>
             {
                IdeRouting.Register(f.Builder, "Home", string.Empty);
             }
         );

         Instance.Run(app, env);
      }
   }
}
