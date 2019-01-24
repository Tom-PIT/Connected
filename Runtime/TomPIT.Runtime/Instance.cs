using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
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
		Jwt = 1,
		Bearer = 2
	}

	public static class Instance
	{

		public static void Initialize(IServiceCollection services, ServicesConfigurationArgs e)
		{
			Shell.RegisterConfigurationType(typeof(ClientSys));
			ConfigureServices(services, e);
		}

		public static void Configure(InstanceType type, IApplicationBuilder app, IHostingEnvironment env, ConfigureRoutingHandler routingHandler)
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
				case AuthenticationType.Jwt:
					services.AddAuthentication(options =>
					{
						options.DefaultAuthenticateScheme = "TomPIT";
						options.DefaultChallengeScheme = "TomPIT";
						options.DefaultScheme = "TomPIT";
					}).AddScheme<JwtAuthenticationOptions, JwtAuthenticationHandler>("TomPIT", "Tom PIT", o =>
					{

					});
					break;
				case AuthenticationType.Bearer:
					services.AddAuthentication(options =>
					{
						options.DefaultAuthenticateScheme = "TomPIT";
						options.DefaultChallengeScheme = "TomPIT";
						options.DefaultScheme = "TomPIT";
					}).AddScheme<BearerAuthenticationOptions, BearerAuthenticationHandler>("TomPIT", "Tom PIT", o =>
					{

					});
					break;
			}

			services.AddMvc((o) =>
			{
				e.ConfigureMvc?.Invoke(o);

				o.Filters.Add(new AuthenticationCookieFilter());
			});

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
	}
}
