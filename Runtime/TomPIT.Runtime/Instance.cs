using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using TomPIT.Connectivity;
using TomPIT.Data.DataProviders;
using TomPIT.DataProviders.Sql;
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
		private static List<string> _resourceGroups = null;

		public static void Initialize(IServiceCollection services, AuthenticationType authentication)
		{
			ConfigureServices(services, authentication);
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
		}

		private static void OnConnectionInitializing(object sender, SysConnectionRegisteredArgs e)
		{
			e.Connection.GetService<IDataProviderService>().Register(new SqlDataProvider());
		}

		private static void ConfigureServices(IServiceCollection services, AuthenticationType authentication)
		{
			switch (authentication)
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

			services.AddMvc(o =>
			{
				var policy = new AuthorizationPolicyBuilder()
				.RequireAuthenticatedUser()
				.Build();

				o.Filters.Add(new AuthorizeFilter(policy));
				o.Filters.Add(new AuthenticationCookieFilter());
			});
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

		public static List<string> ResourceGroups
		{
			get
			{
				if (_resourceGroups == null)
					_resourceGroups = new List<string>();

				return _resourceGroups;
			}
		}

		public static void Run(IApplicationBuilder app)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("sys.json");

			var config = builder.Build();

			var sys = config.GetSection("connections");
			var items = sys.GetChildren();

			foreach (var i in items)
			{
				var name = i.GetSection("name");
				var url = i.GetSection("url");
				var clientKey = i.GetSection("clientKey");

				Shell.GetService<IConnectivityService>().Insert(name.Value, url.Value, clientKey.Value);
			}

			var groups = config.GetSection("resourceGroups");

			if (groups != null)
			{
				var c = groups.GetChildren();

				foreach (var i in c)
					ResourceGroups.Add(i.Value);
			}
			else
				ResourceGroups.Add("Default");
		}
	}
}
