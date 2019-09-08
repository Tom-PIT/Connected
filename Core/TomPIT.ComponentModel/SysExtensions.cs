using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Text;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Security;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT
{
	public static class SysExtensions
	{
		public static IUser AmtShell { get; private set; }

		public static ISysConnection Connection(this IExecutionContext context)
		{
			if (context is IEndpointContext enc && !string.IsNullOrWhiteSpace(enc.Endpoint))
				return Shell.GetService<IConnectivityService>().Select(enc.Endpoint);

			var identity = GetIdentity();

			if (identity != null && identity is Identity id)
				return Shell.GetService<IConnectivityService>().Select(id.Endpoint);

			return Shell.GetService<IConnectivityService>().Select();
		}

		public static Guid GetAuthenticatedUserToken(this ISysConnection connection)
		{
			var u = GetAuthenticatedUser(connection);

			if (u == null)
				return Guid.Empty;

			return u.Token;
		}

		public static Guid GetAuthenticatedUserToken(this IExecutionContext context)
		{
			return GetAuthenticatedUserToken(context.Connection());
		}

		public static IUser GetAuthenticatedUser(this IExecutionContext context)
		{
			return GetAuthenticatedUser(context.Connection());
		}

		public static IUser GetAuthenticatedUser(this ISysConnection connection)
		{
			var identity = GetIdentity();

			if (identity == null)
				return null;

			if (Guid.TryParse(identity.Name, out Guid token))
				return connection.GetService<IUserService>().SelectByAuthenticationToken(token);

			return null;
		}

		public static Guid CurrentUserToken(this HttpContext context)
		{
			var u = CurrentUser(context);

			return u == null
				? Guid.Empty
				: u.Token;
		}

		public static IUser CurrentUser(this HttpContext context)
		{
			if (context == null || context.User == null || context.User.Identity == null)
				return null;

			if (!context.User.Identity.IsAuthenticated)
				return null;

			if (!(context.User.Identity is Identity identity))
				return null;

			return identity.User;
		}

		public static void WithRequestArguments(this JObject instance, NameValueCollection items)
		{
			if (items == null)
				return;

			foreach (var i in items.Keys)
			{
				var sk = Types.Convert<string>(i);

				if (!Types.IsValueDefined(sk))
					continue;

				instance.Add(sk, items[sk]);
			}
		}

		public static string JwToken(this HttpRequest request)
		{
			if (request == null)
				return null;

			if (!request.Cookies.ContainsKey(SecurityUtils.AuthenticationCookieName))
				return null;

			var cookie = request.Cookies[SecurityUtils.AuthenticationCookieName];
			var json = Types.Deserialize<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(cookie)));

			return json.Required<string>("jwt");
		}

		public static string JwToken()
		{
			return JwToken(Shell.HttpContext?.Request);
		}

		public static IIdentity GetIdentity(this IExecutionContext context)
		{
			return GetIdentity();
		}

		public static IIdentity GetIdentity()
		{
			if (Shell.HttpContext == null)
				return null;

			var u = Shell.HttpContext.User;

			if (u == null)
				return null;
			else
				return u.Identity;
		}

		public static ISysConnection CurrentConnection()
		{
			if (!(Shell.HttpContext.User.Identity is Identity identity) || string.IsNullOrWhiteSpace(identity.Endpoint))
			{
				if(Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
					return Shell.GetService<IConnectivityService>().Select();

				return null;
			}

			return Shell.GetService<IConnectivityService>().Select(identity.Endpoint);
		}

		public static Guid ResolveMicroServiceToken(this ISysConnection context, string microService)
		{
			var r = ResolveMicroService(context, microService);

			return r.Token;
		}

		public static string ResolveMicroServiceName(this ISysConnection context, Guid microService)
		{
			var r = ResolveMicroService(context, microService);

			return r.Name;
		}

		public static IMicroService ResolveMicroService(this ISysConnection context, string microService)
		{
			return context.GetService<IMicroServiceService>().Select(microService);
		}

		public static IMicroService ResolveMicroService(this ISysConnection context, Guid microService)
		{
			return context.GetService<IMicroServiceService>().Select(microService);
		}

		public static IContextServices CreateServices(this IExecutionContext context, HttpRequest request)
		{
			return new ContextServices(context);
		}
	}
}
