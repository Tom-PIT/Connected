using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Routing;
using TomPIT.Services;

namespace TomPIT
{
	public static class RuntimeExtensions
	{
		internal static bool ContainsQueryParameter(this HttpContext context, string key)
		{
			if (context == null)
				return false;

			var q = context.Request.Query;

			return q.ContainsKey(key);
		}

		internal static string QueryParameter(this HttpContext context, string key)
		{
			if (!ContainsQueryParameter(context, key))
				return null;

			var q = context.Request.Query;

			if (q.ContainsKey(key))
				return q[key];

			return null;
		}

		public static ServerUrl CreateUrl(this ISysConnection connection, string baseUrl, string microService, string api, string operation)
		{
			return new ApiUrl(baseUrl, microService, api, operation);
		}

		public static bool IsMicroServiceSupported(this ISysConnection connection, Guid microService)
		{
			var ms = connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return false;

			return Instance.ResourceGroupExists(ms.ResourceGroup);
		}

		public static IDataModelContext CreateContext(this IComponent component)
		{
			return new DataModelContext(ExecutionContext.Create(Instance.Connection.Url, Instance.Connection.GetService<IMicroServiceService>().Select(component.MicroService)));
		}

		public static IDataModelContext CreateContext(this IConfiguration configuration)
		{
			return CreateContext(Instance.GetService<IComponentService>().SelectComponent(configuration.Component));
		}

		public static dynamic CreateInstance(this Type type, IDataModelContext context, object[] ctorArgs)
		{
			dynamic instance = type.CreateInstance(ctorArgs);

			if (instance is IProcessHandler ph)
				ph.Initialize(context);

			return instance;
		}

		public static dynamic CreateInstance(this Type type, IDataModelContext context)
		{
			return type.CreateInstance(context, null);
		}

		public static void RemoveSystemLogin(this IRouteBuilder builder)
		{
			var toRemove = new List<Microsoft.AspNetCore.Routing.Route>();

			foreach (var route in builder.Routes)
			{
				if (!(route is Microsoft.AspNetCore.Routing.Route r) || string.IsNullOrWhiteSpace(r.Name))
					continue;

				if (r.Name.StartsWith("login"))
					toRemove.Add(r);
			}

			foreach (var route in toRemove)
				builder.Routes.Remove(route);
		}

		public static void AddSystemLogin(this IRouteBuilder builder)
		{
			foreach (var route in builder.Routes)
			{
				if (!(route is Microsoft.AspNetCore.Routing.Route r) || string.IsNullOrWhiteSpace(r.Name))
					continue;

				if (r.Name.StartsWith("login"))
					return;
			}

			var loginController = "Login";

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
				loginController = "MultiTenantLogin";

			builder.MapRoute("login", "login", new { controller = loginController, action = "Index" });
			builder.MapRoute("login.authenticate", "login/authenticate", new { controller = loginController, action = "Authenticate" });
			builder.MapRoute("login.changepassword", "login/change-password", new { controller = loginController, action = "ChangePassword" });

			foreach(var route in builder.Routes)
			{
				if (route is Microsoft.AspNetCore.Routing.Route r && !string.IsNullOrEmpty(r.Name) && string.Compare(r.Name, "logoff", true) == 0)
					return;
			}

			builder.MapRoute("logoff", "logoff", new { controller = loginController, action = "Logoff" });
		}
	}
}