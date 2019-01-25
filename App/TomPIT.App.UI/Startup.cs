using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Globalization;
using TomPIT.Resources;
using TomPIT.Themes;
using TomPIT.UI;

namespace TomPIT
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			var e = new ServicesConfigurationArgs
			{
				Authentication = AuthenticationType.MultiTenant
			};

			Instance.Initialize(services, e);

			services.AddScoped<IViewEngine, ViewEngine>();

			services.Configure<RazorViewEngineOptions>(opts =>
			{
				opts.FileProviders.Add(
					new ViewProvider()
			  );
			}
			);
		}


		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			Instance.Configure(InstanceType.Application, app, env, (f) =>
			{
				app.UseMiddleware<IgnoreRouteMiddleware>();

				Configuration.Routing.Register(f.Builder);
			});

			InitializeConfiguration();
			Instance.Run(app);
		}

		private void InitializeConfiguration()
		{
			Shell.GetService<IConnectivityService>().ConnectionRegistered += OnConnectionRegistered;
		}

		private void OnConnectionRegistered(object sender, SysConnectionRegisteredArgs e)
		{
			e.Connection.RegisterService(typeof(IViewService), typeof(ViewService));
			e.Connection.RegisterService(typeof(IThemeService), typeof(ThemeService));
			e.Connection.RegisterService(typeof(IResourceService), typeof(ResourceService));
			e.Connection.RegisterService(typeof(IClientGlobalizationService), typeof(ClientGlobalizationService));
		}
	}
}
