using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT
{
	public static class SysExtensions
	{
		public static IUser AmtShell { get; private set; }

		public static ISysConnection Connection(this IExecutionContext context)
		{
			if (context is IEndpointContext enc && !string.IsNullOrWhiteSpace(enc.Endpoint))
				return Shell.GetService<IConnectivityService>().Select(enc.Endpoint);

			var identity = context.GetIdentity();

			if (identity != null && identity is Identity id)
				return Shell.GetService<IConnectivityService>().Select(id.Endpoint);

			return Shell.GetService<IConnectivityService>().Select();
		}

		public static void SetContextId(this IContextIdentity identity, string contextId)
		{
			if (identity is ContextIdentity s)
				s.ContextId = contextId;
		}

		public static Guid GetAuthenticatedUserToken(this IExecutionContext context)
		{
			var u = GetAuthenticatedUser(context);

			if (u == null)
				return Guid.Empty;

			return u.Token;
		}

		public static IUser GetAuthenticatedUser(this IExecutionContext context)
		{
			var identity = context.GetIdentity();

			if (identity == null)
				return null;

			if (Guid.TryParse(identity.Name, out Guid token))
			{
				var ctx = context.Connection();

				if (ctx != null)
					return ctx.GetService<IUserService>().SelectByAuthenticationToken(token);
			}

			return null;
		}

		public static HttpRequest GetHttpRequest(this IExecutionContext context)
		{
			if (!(context is IRequestContextProvider ctx))
			{
				if (context is ExecutionContext)
					return ((ExecutionContext)context).Request;

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

			return json.Required<string>("jwt");
		}

		public static string JwToken(this IExecutionContext context)
		{
			return JwToken(GetHttpRequest(context));
		}

		public static IIdentity GetIdentity(this IExecutionContext context)
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
	}
}
