using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Data.DataProviders;
using TomPIT.Design;
using TomPIT.Distributed;
using TomPIT.Environment;
using TomPIT.Globalization;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;
using TomPIT.Security;
using TomPIT.Security.Authentication;

namespace TomPIT
{
	public delegate void ConfigureRoutingHandler(ConfigureRoutingArgs e);

	public enum AuthenticationType
	{
		None = 0,
		MultiTenant = 1,
		SingleTenant = 2
	}

	public enum InstanceState
	{
		Initializing = 1,
		Running = 2
	}
	public static class Instance
	{
		public static IMvcBuilder Mvc { get; private set; }
		private static List<IPlugin> _plugins = null;
		internal static RequestLocalizationOptions RequestLocalizationOptions { get; private set; }
		public static Guid Id { get; } = Guid.NewGuid();
		public static InstanceState State { get; private set; } = InstanceState.Initializing;
		public static CancellationToken Stopping { get; private set; }
		public static CancellationToken Stopped { get; private set; }

		private static bool CorsEnabled { get; set; }
		public static void Initialize(IServiceCollection services, ServicesConfigurationArgs e)
		{
			Shell.RegisterConfigurationType(typeof(ClientSys));

			InitializeServices(services, e);
		}

		private static void InitializeServices(IServiceCollection services, ServicesConfigurationArgs e)
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

			Mvc = services.AddMvc((o) =>
			{
				e.ConfigureMvc?.Invoke(o);
			}).AddNewtonsoftJson()
			.ConfigureApplicationPartManager((m) =>
			{
				var pa = new ApplicationPartsArgs();

				foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
				{
					var t = Reflection.TypeExtensions.GetType(i);

					if (t == null)
						continue;

					var template = t.CreateInstance<IMicroServiceTemplate>();

					var ds = template.GetApplicationParts();

					if (ds != null && ds.Count > 0)
						pa.Parts.AddRange(ds);
				}

				e.ProvideApplicationParts?.Invoke(pa);

				foreach (var assembly in pa.Assemblies)
					m.ApplicationParts.Add(new AssemblyPart(assembly));

				foreach (var i in pa.Parts)
					ConfigurePlugins(m, i);

				foreach (var i in Shell.GetConfiguration<IClientSys>().Plugins.Items)
				{
					var t = Reflection.TypeExtensions.GetType(i);

					if (t == null)
						continue;

					var plugin = t.CreateInstance<IPlugin>();

					var parts = plugin.GetApplicationParts(m);

					if (parts != null)
					{
						foreach (var j in parts)
							ConfigurePlugins(m, j);
					}
				}
			});

			if (e.CorsEnabled)
			{
				CorsEnabled = true;

				services.AddCors(options => options.AddPolicy("TomPITPolicy",
					builder =>
					{
						var setting = MiddlewareDescriptor.Current.Tenant.GetService<ISettingService>().Select("Cors Origins", null, null, null);
						var origin = new string[] { "http://localhost" };

						if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
							origin = setting.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

						builder.AllowAnyMethod()
						.AllowAnyHeader()
						.WithOrigins(origin)
						.AllowCredentials();
					}));
			}

			services.AddControllersWithViews();
			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

			services.AddAuthorization(options =>
			{
				options.AddPolicy(Claims.ImplementMicroservice, policy => policy.RequireClaim(Claims.ImplementMicroservice));
			});


			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddSingleton<IAuthorizationHandler, ClaimHandler>();
			services.AddSingleton<IHostedService, FlushingService>();

			foreach (var plugin in Plugins)
				plugin.ConfigureServices(services);
		}
		public static void Configure(InstanceType type, IApplicationBuilder app, IWebHostEnvironment env, ConfigureRoutingHandler routingHandler)
		{
			RuntimeService._host = app;
			app.UseMiddleware<AuthenticationCookieMiddleware>();

			var lifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();

			if (lifetime != null)
			{
				Stopping = lifetime.ApplicationStopping;
				Stopped = lifetime.ApplicationStopped;
			}
			ConfigureStaticFiles(app, env);

			app.UseStatusCodePagesWithReExecute("/sys/status/{0}");
			app.UseRouting();

			if (CorsEnabled)
				app.UseCors("TomPITPolicy");

			app.UseAuthentication();
			app.UseAuthorization();
			app.UseRequestLocalization(o =>
			{
				RequestLocalizationOptions = o;
				o.DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture);
				o.FallBackToParentCultures = true;
				o.FallBackToParentUICultures = true;
				/*
				 * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-2.2
				 */
				o.RequestCultureProviders.Insert(1, new IdentityCultureProvider());
			});

			app.UseAjaxExceptionMiddleware();

			RuntimeBootstrapper.Run();

			Shell.GetService<IRuntimeService>().Initialize(type, Shell.GetConfiguration<IClientSys>().Platform, env);
			Shell.GetService<IConnectivityService>().TenantInitialized += OnTenantInitialized;
			app.UseEndpoints(routes =>
			{
				foreach (var i in Plugins)
					i.RegisterRoutes(routes);

				RoutingConfiguration.Register(routes);
				routingHandler?.Invoke(new ConfigureRoutingArgs(routes));
			});

			Shell.Configure(app);

			foreach (var plugin in Plugins)
				plugin.Initialize(app, env);
		}

		private static void ConfigureStaticFiles(IApplicationBuilder app, IWebHostEnvironment env)
		{
			var cachePeriod = env.IsDevelopment() ? "600" : "604800";
			var contentTypeProvider = new FileExtensionContentTypeProvider();

			contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";

			var staticOptions = new StaticFileOptions
			{
				OnPrepareResponse = ctx =>
				{
					ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
				},
				ContentTypeProvider = contentTypeProvider,
			};

			EmbeddedResourcesConfiguration.Configure(env, staticOptions);

			app.UseStaticFiles(staticOptions);
		}
		private static void OnTenantInitialized(object sender, TenantArgs e)
		{
			foreach (var i in Shell.GetConfiguration<IClientSys>().DataProviders)
			{
				var t = Reflection.TypeExtensions.GetType(i);

				if (t == null)
					continue;

				var provider = t.CreateInstance<IDataProvider>();

				if (provider != null)
					e.Tenant.GetService<IDataProviderService>().Register(provider);
			}
		}

		public static void Run(IApplicationBuilder app, IWebHostEnvironment environment)
		{
			foreach (var i in Shell.GetConfiguration<IClientSys>().Connections)
				Shell.GetService<IConnectivityService>().InsertTenant(i.Name, i.Url, i.AuthenticationToken);

			State = InstanceState.Running;

			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
			{
				var t = Reflection.TypeExtensions.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				template.Initialize(app, environment);
			}
		}

		public static bool ResourceGroupExists(Guid resourceGroup)
		{
			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
				return true;

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
			{
				var rg = MiddlewareDescriptor.Current.Tenant.GetService<IResourceGroupService>().Select(i);

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
						var t = Reflection.TypeExtensions.GetType(i);

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
