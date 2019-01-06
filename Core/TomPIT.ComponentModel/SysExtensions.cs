using System;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Net;
using TomPIT.Runtime;
using TomPIT.Security;

namespace TomPIT
{
	public static class SysExtensions
	{
		public static IUser AmtShell { get; private set; }

		public static ISysContext GetServerContext(this IApplicationContext context)
		{
			if (context is IEndpointContext enc && !string.IsNullOrWhiteSpace(enc.Endpoint))
				return Shell.GetService<IConnectivityService>().Select(enc.Endpoint);

			var identity = context.GetIdentity();

			if (identity != null && identity is Identity id)
				return Shell.GetService<IConnectivityService>().Select(id.Endpoint);

			return Shell.GetService<IConnectivityService>().Select();
		}

		public static Guid GetAuthenticatedUserToken(this IApplicationContext context)
		{
			var u = GetAuthenticatedUser(context);

			if (u == null)
				return Guid.Empty;

			return u.Token;
		}

		public static IUser GetAuthenticatedUser(this IApplicationContext context)
		{
			var identity = context.GetIdentity();

			if (identity == null)
				return null;

			if (Guid.TryParse(identity.Name, out Guid token))
			{
				var ctx = context.GetServerContext();

				if (ctx != null)
					return ctx.GetService<IUserService>().SelectByAuthenticationToken(token);
			}

			return null;
		}

		public static HttpRequest GetHttpRequest(this IApplicationContext context)
		{
			if (!(context is IRequestContextProvider ctx))
			{
				if (context is ApplicationContext)
					return ((ApplicationContext)context).Request;

				return null;
			}

			return ctx.Request;
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
			var json = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(cookie)));

			return json.Argument<string>("jwt");
		}

		public static string JwToken(this IApplicationContext context)
		{
			return JwToken(GetHttpRequest(context));
		}

		public static IIdentity GetIdentity(this IApplicationContext context)
		{
			if (!(context is IRequestContextProvider provider))
				return null;

			if (provider.Request == null)
				return null;

			var u = provider.Request.HttpContext.User;

			if (u == null)
				return null;
			else
				return u.Identity;
		}

		public static Guid ResolveMicroServiceToken(this ISysContext context, string microService)
		{
			var r = ResolveMicroService(context, microService);

			return r.Token;
		}

		public static string ResolveMicroServiceName(this ISysContext context, Guid microService)
		{
			var r = ResolveMicroService(context, microService);

			return r.Name;
		}

		public static IMicroService ResolveMicroService(this ISysContext context, string microService)
		{
			return context.GetService<IMicroServiceService>().Select(microService);
		}

		public static IMicroService ResolveMicroService(this ISysContext context, Guid microService)
		{
			return context.GetService<IMicroServiceService>().Select(microService);
		}
	}
}
