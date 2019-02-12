using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Data.DataProviders;
using TomPIT.Environment;
using TomPIT.Runtime;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT
{
	public delegate void ConfigureRoutingHandler(ConfigureRoutingArgs e);

	public enum AuthenticationType
	{
		None = 0,
		MultiTenant = 1,
		SingleTenant = 2
	}

	public static class Instance
	{
		private static IMvcBuilder _mvcBuilder = null;
		private static List<IPlugin> _plugins = null;

		public static Guid Id { get; } = Guid.NewGuid();

		public static void Initialize(IServiceCollection services, ServicesConfigurationArgs e)
		{
			Shell.RegisterConfigurationType(typeof(ClientSys));

			services.AddSingleton<IHostedService, FlushingService>();
			ConfigureServices(services, e);
		}

		public static void Configure(InstanceType type, IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ConfigureRoutingHandler routingHandler)
		{
			app.UseAjaxExceptionMiddleware();
			app.UseStaticFiles();
			app.UseAuthentication();
			app.UseStatusCodePagesWithReExecute("/sys/status/{0}");

			RuntimeBootstrapper.Run();

			Shell.GetService<IRuntimeService>().Initialize(type, env);
			Shell.GetService<IConnectivityService>().ConnectionInitializing += OnConnectionInitializing;

			app.UseMvc(routes =>
			{
				foreach (var i in Plugins)
					i.RegisterRoutes(routes);

				RoutingConfiguration.Register(routes);
				routingHandler?.Invoke(new ConfigureRoutingArgs(routes));
			});

			Shell.Configure(app);
		}

		private static void OnConnectionInitializing(object sender, SysConnectionRegisteredArgs e)
		{
			foreach (var i in Shell.GetConfiguration<IClientSys>().DataProviders)
			{
				var t = Types.GetType(i);

				if (t == null)
					continue;

				var provider = t.CreateInstance<IDataProvider>();

				if (provider != null)
					e.Connection.GetService<IDataProviderService>().Register(provider);
			}
		}

		private static void ConfigureServices(IServiceCollection services, ServicesConfigurationArgs e)
		{
			switch (e.Authentication)
			{
				case AuthenticationType.MultiTenant:
					services.AddAuthentication(options =>
					{
						options.DefaultAuthenticateScheme = "TomPIT";
						options.DefaultChallengeScheme = "TomPIT";
						options.DefaultScheme = "TomPIT";
					}).AddScheme<MultiTenantAuthenticationOptions, MultiTenantAuthenticationHandler>("TomPIT", "Tom PIT", o =>
					{

					});
					break;
				case AuthenticationType.SingleTenant:
					services.AddAuthentication(options =>
					{
						options.DefaultAuthenticateScheme = "TomPIT";
						options.DefaultChallengeScheme = "TomPIT";
						options.DefaultScheme = "TomPIT";
					}).AddScheme<SingleTenantAuthenticationOptions, SingleTenantAuthenticationHandler>("TomPIT", "Tom PIT", o =>
					{

					});
					break;
			}

			_mvcBuilder = services.AddMvc((o) =>
			{
				e.ConfigureMvc?.Invoke(o);

				o.Filters.Add(new AuthenticationCookieFilter());
			}).ConfigureApplicationPartManager((m) =>
			{
				var pa = new ApplicationPartsArgs();

				e.ProvideApplicationParts?.Invoke(pa);

				foreach (var i in pa.Parts)
					ConfigurePlugins(m, i);

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
							ConfigurePlugins(m, j);
					}
				}
			});

			_mvcBuilder.AddControllersAsServices();

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.ConfigureOptions(typeof(EmbeddedResourcesConfiguration));
		}

		public static T GetService<T>()
		{
			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
				throw new RuntimeException(SR.ErrSingleTenantOnly);

			return Shell.GetService<IConnectivityService>().Select().GetService<T>();
		}

		public static ISysConnection Connection
		{
			get
			{
				if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
					throw new RuntimeException(SR.ErrSingleTenantOnly);

				return Shell.GetService<IConnectivityService>().Select();
			}
		}

		public static void Run(IApplicationBuilder app)
		{
			foreach (var i in Shell.GetConfiguration<IClientSys>().Connections)
				Shell.GetService<IConnectivityService>().Insert(i.Name, i.Url, i.AuthenticationToken);
		}

		public static bool ResourceGroupExists(Guid resourceGroup)
		{
			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
				return true;

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
			{
				var rg = Connection.GetService<IResourceGroupService>().Select(i);

				if (rg != null)
					return true;
			}

			return false;
		}

		public static List<IPlugin> Plugins
		{
			get
			{
				if (_plugins == null)
				{
					_plugins = new List<IPlugin>();

					foreach (var i in Shell.GetConfiguration<IClientSys>().Plugins.Items)
					{
						var t = Types.GetType(i);

						if (t == null)
							continue;

						var plugin = t.CreateInstance<IPlugin>();

						if (plugin != null)
							_plugins.Add(plugin);
					}
				}

				return _plugins;
			}
		}

		private static void ConfigurePlugins(ApplicationPartManager manager, string assembly)
		{
			var path = Shell.ResolveAssemblyPath(assembly);

			if (path == null)
				return;

			var asmName = AssemblyName.GetAssemblyName(path);
			var asm = AssemblyLoadContext.Default.LoadFromAssemblyName(asmName);

			AddApplicationPart(manager, asm);

			var relatedAssemblies = RelatedAssemblyAttribute.GetRelatedAssemblies(asm, false);

			foreach (var i in relatedAssemblies)
				AddApplicationPart(manager, i);
		}

		private static void AddApplicationPart(ApplicationPartManager manager, Assembly assembly)
		{
			var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);

			foreach (var i in partFactory.GetApplicationParts(assembly))
				manager.ApplicationParts.Add(i);
		}
	}
}
