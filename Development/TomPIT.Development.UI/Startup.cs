using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.Loader;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Environment;
using TomPIT.Services;

namespace TomPIT.Servers.Development
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
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			Instance.Configure(InstanceType.Development, app, env, (f) =>
		  {
			  Configuration.Routing.Register(f.Builder);
			  IdeRouting.Register(f.Builder, "Ide", "ide/{microservice}");
		  });

			DevelopmentBootstrapper.Run();
			IdeBootstrapper.Run();
			Shell.GetService<IConnectivityService>().ConnectionInitializing += OnConnectionInitializing;
			Instance.Run(app);

			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
			{
				var t = Types.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				var parts = template.GetApplicationParts();

				if (parts != null)
				{
					foreach (var j in parts)
						ConfigurePlugins(j);
				}
			}
		}

		private static void OnConnectionInitializing(object sender, SysConnectionRegisteredArgs e)
		{
			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
			{
				var t = Types.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				if (template != null)
					e.Connection.GetService<IMicroServiceTemplateService>().Register(template);
			}
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
