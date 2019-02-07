using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.Loader;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Globalization;
using TomPIT.Resources;
using TomPIT.Services;
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

			foreach (var i in Shell.GetConfiguration<IClientSys>().Plugins.Items)
			{
				var t = Types.GetType(i);

				if (t == null)
					continue;

				var plugin = t.CreateInstance<IPlugin>();

				var parts = plugin.GetApplicationParts();

				if (parts != null)
				{
					foreach (var j in parts)
						ConfigurePlugins(j);
				}
			}
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

		private void ConfigurePlugins(string assembly)
		{
			var path = Shell.ResolveAssemblyPath(assembly);

			if (path == null)
				return;

			var asmName = AssemblyName.GetAssemblyName(path);
			var asm = AssemblyLoadContext.Default.LoadFromAssemblyName(asmName);

			Instance.AddApplicationPart(asm);

			var relatedAssemblies = RelatedAssemblyAttribute.GetRelatedAssemblies(asm, true);

			foreach (var i in relatedAssemblies)
				Instance.AddApplicationPart(i);
		}
	}
}
