using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Distributed;
using TomPIT.Environment;
using TomPIT.Globalization;
using TomPIT.Middleware;
using TomPIT.Proxy.Local;
using TomPIT.Proxy.Remote;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Security;
using TomPIT.Security.Authentication;
using TomPIT.Startup;

namespace TomPIT;
internal class StartupHost : IStartupHostProxy
{
	public event EventHandler<IServiceCollection> ConfiguringServices;
	public event EventHandler<Tuple<IApplicationBuilder, IWebHostEnvironment>> Configuring;
	public event EventHandler Booting;
	public event EventHandler SysProxyCreated;
	public event EventHandler<HubOptions> ConfiguringSignalR;
	public event EventHandler<IEndpointRouteBuilder> ConfiguringRouting;
	public event EventHandler<IRouteBuilder> ConfiguringMvcRouting;
	public event EventHandler<MvcOptions> ConfiguringMvc;
	public event EventHandler<ApplicationPartsArgs> ConfiguringApplicationParts;
	public event EventHandler<List<Assembly>> ConfigureEmbeddedStaticResources;

	public event EventHandler<IMvcBuilder> MvcConfigured;

	public void ConfigureServices(IServiceCollection services)
	{
		RuntimeBootstrapper.Run();
		Boot();
		Shell.GetService<IConnectivityService>().TenantInitialized += OnTenantInitialized;
		ConfigureTenant();
		ConfigureLocalization(services);
		ConfigureAuthentication(services);
		ConfigureMvc(services);
		ConfigureCors(services);
		ConfigureAuthorization(services);
		ConfigureSignalR(services);
		ConfigurePlugins(services);

		services.AddControllersWithViews();
		services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		services.AddSingleton<IAuthorizationHandler, ClaimHandler>();
		services.AddSingleton<IHostedService, FlushingService>();
		services.AddScoped<RequestLocalizationCookiesMiddleware>();
		services.AddHttpClient();

		ConfiguringServices?.Invoke(null, services);

		foreach (var startup in MicroServices.Startups)
			startup.ConfigureServices(services);
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		RuntimeService._host = app;
		app.UseAuthentication();
		app.UseMiddleware<AuthenticationCookieMiddleware>();
		app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>()?.Value);
		app.UseResponseCompression();

		var lifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();

		if (lifetime is not null)
		{
			Instance.Stopping = lifetime.ApplicationStopping;
			Instance.Stopped = lifetime.ApplicationStopped;
		}

		ConfigureStaticFiles(app, env);

		app.UseStatusCodePagesWithReExecute("/sys/status/{0}");

		Configuring?.Invoke(null, new(app, env));

		app.UseRouting();

		app.UseAuthorization();

		if (Types.TryConvert(Tenant.GetService<ISettingService>().Select("Cors Enabled", null, null, null)?.Value, out bool corsEnabled) && corsEnabled)
			app.UseCors("TomPITPolicy");

		app.UseRequestLocalizationCookies();
		app.UseAjaxExceptionMiddleware();

		Shell.GetService<IRuntimeService>().Initialize(env);

		if (MiddlewareDescriptor.Current?.Tenant?.GetService<IMicroServiceRuntimeService>() is IMicroServiceRuntimeService runtimeService)
			runtimeService.Configure(app);

		app.UseEndpoints(routes =>
		{
			foreach (var i in Instance.Plugins)
				i.RegisterRoutes(routes);

			RoutingConfiguration.Register(routes);
			ConfiguringRouting?.Invoke(this, routes);
		});

		app.UseMvc(r =>
		{
			ConfiguringMvcRouting?.Invoke(null, r);
		});

		Shell.Configure(app);

		foreach (var plugin in Instance.Plugins)
			plugin.Initialize(app, env);

		Run(app, env);

		foreach (var startup in MicroServices.Startups)
			startup.Configure(app, env);
	}

	private void Boot()
	{
		Booting?.Invoke(null, EventArgs.Empty);

		if (Instance.Features.HasFlag(InstanceFeatures.Sys))
			Instance.SysProxy = new LocalProxy();
		else
			Instance.SysProxy = new RemoteProxy();

		SysProxyCreated?.Invoke(null, EventArgs.Empty);
	}

	private void ConfigureTenant()
	{
		if (Shell.Configuration.GetSection("sys") is null && !Instance.Features.HasFlag(InstanceFeatures.Sys))
			throw new ConfigurationErrorsException("'sys' configuration element expected.");

		var bindings = new ConfigurationBindings();

		Shell.Configuration.Bind("sys", bindings);

		Shell.GetService<IConnectivityService>().InsertTenant(bindings.Name, bindings.Url, bindings.Token);
	}

	private class ConfigurationBindings 
	{
		public string? Name { get; set; }
		public string? Url { get; set; }
		public string? Token { get; set;  }
	}

	private void OnTenantInitialized(object sender, TenantArgs e)
	{
		e.Tenant.GetService<IDesignService>().Initialize();
		/*
			* Attempt to access settings from Sys. These settings are required by CORS settings, and the instance cannot work 
			* properly if these are not set at startup. If sys is unavailable, shutdown and try again.
			*/
		Tenant.GetService<ISettingService>().Select("Cors Origins", null, null, null);
	}

	private void ConfigureLocalization(IServiceCollection services)
	{
		services.Configure<RequestLocalizationOptions>(o =>
		{
			Instance.RequestLocalizationOptions = o;
			o.DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture);
			o.FallBackToParentCultures = true;
			o.FallBackToParentUICultures = true;
			/*
				 * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-3.1
				 */
			o.RequestCultureProviders.Insert(2, new DefaultSettingsCultureProvider());
			o.RequestCultureProviders.Insert(2, new DomainCultureProvider());
			o.RequestCultureProviders.Insert(1, new IdentityCultureProvider());

			var instances = o.RequestCultureProviders.Where(f => f.GetType() == typeof(AcceptLanguageHeaderRequestCultureProvider)).ToList();

			instances.ForEach(obj => o.RequestCultureProviders.Remove(obj));

			Tenant.GetService<ILanguageService>().ApplySupportedCultures();
		});
	}

	private void ConfigureAuthentication(IServiceCollection services)
	{
		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = "TomPIT";
			options.DefaultChallengeScheme = "TomPIT";
			options.DefaultScheme = "TomPIT";
		}).AddScheme<SingleTenantAuthenticationOptions, SingleTenantAuthenticationHandler>("TomPIT", "Tom PIT", o =>
		{

		});
	}

	private void ConfigureMvc(IServiceCollection services)
	{
		var builder = services.AddMvc((o) =>
		{
			o.EnableEndpointRouting = false;

			ConfiguringMvc?.Invoke(null, o);
		});

		builder.AddNewtonsoftJson();
		builder.ConfigureApplicationPartManager((m) =>
			{
				var args = new ApplicationPartsArgs();

				foreach (var i in Tenant.GetService<IDesignService>().QueryDesigners())
				{
					var t = Reflection.TypeExtensions.GetType(i);

					if (t is null)
						continue;

					var template = t.CreateInstance<IMicroServiceTemplate>();

					var ds = template.GetApplicationParts();

					if (ds is not null && ds.Any())
						args.Parts.AddRange(ds);
				}

				ConfiguringApplicationParts?.Invoke(null, args);

				foreach (var assembly in args.Assemblies)
					m.ApplicationParts.Add(new AssemblyPart(assembly));

				foreach (var i in args.Parts)
					ConfigurePlugins(m, i);

				foreach (var i in Instance.Plugins)
				{
					var parts = i.GetApplicationParts(m);

					if (parts is not null)
					{
						foreach (var j in parts)
							ConfigurePlugins(m, j);
					}
				}
			});

		MvcConfigured?.Invoke(null, builder);
	}

	private void ConfigureCors(IServiceCollection services)
	{
		if (Tenant.GetService<ISettingService>().GetValue<bool>("Cors Enabled", null, null, null))
		{
			services.AddCors(options => options.AddPolicy("TomPITPolicy",
				 builder =>
				 {
					 var setting = Tenant.GetService<ISettingService>().Select("Cors Origins", null, null, null);
					 var origin = new string[] { "http://localhost" };

					 if (setting is not null && !string.IsNullOrWhiteSpace(setting.Value))
						 origin = setting.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

					 builder.AllowAnyMethod()
						  .AllowAnyHeader()
						  .WithOrigins(origin)
						  .AllowCredentials();
				 }));
		}
	}

	private void ConfigureAuthorization(IServiceCollection services)
	{
		services.AddAuthorization(options =>
		{
			options.AddPolicy(Claims.ImplementMicroservice, policy => policy.RequireClaim(Claims.ImplementMicroservice));
		});
	}

	private void ConfigureSignalR(IServiceCollection services)
	{
		services.AddSignalR(o =>
		{
			ConfiguringSignalR?.Invoke(null, o);
		}).AddNewtonsoftJsonProtocol();
	}

	private void ConfigurePlugins(IServiceCollection services)
	{
		foreach (var plugin in Instance.Plugins)
			plugin.ConfigureServices(services);
	}

	private void ConfigureStaticFiles(IApplicationBuilder app, IWebHostEnvironment env)
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

		var args = new List<Assembly>();
		ConfigureEmbeddedStaticResources?.Invoke(null, args);

		EmbeddedResourcesConfiguration.Configure(env, staticOptions, args);

		app.UseStaticFiles(staticOptions);
	}

	private void ConfigurePlugins(ApplicationPartManager manager, string assembly)
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

	private void AddApplicationPart(ApplicationPartManager manager, Assembly assembly)
	{
		var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);

		foreach (var i in partFactory.GetApplicationParts(assembly))
			manager.ApplicationParts.Add(i);
	}

	private void Run(IApplicationBuilder app, IWebHostEnvironment environment)
	{
		Instance.State = InstanceState.Running;

		foreach (var i in Tenant.GetService<IDesignService>().QueryDesigners())
		{
			var t = Reflection.TypeExtensions.GetType(i);

			if (t is null)
				continue;

			var template = t.CreateInstance<IMicroServiceTemplate>();

			template.Initialize(app, environment);
		}

		if (Shell.GetService<IRuntimeService>() is RuntimeService runtime)
			runtime.IsInitialized = true;
	}
}
