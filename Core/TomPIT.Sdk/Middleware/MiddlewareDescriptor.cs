using System;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Models;
using TomPIT.Runtime;
using TomPIT.Security;
using TomPIT.Serialization;

namespace TomPIT.Middleware
{
	public class MiddlewareDescriptor : IMiddlewareDescriptor
	{
		private IIdentity _identity = null;
		private IUser _user = null;
		private string _jw = null;
		private ITenant _tenant = null;

		internal MiddlewareDescriptor() { }
		public IIdentity Identity
		{
			get
			{
				if (_identity == null)
				{
					if (Shell.HttpContext == null)
						return null;

					var user = Shell.HttpContext.User;

					if (user == null)
						return null;
					else
						_identity = user.Identity;
				}

				return _identity;
			}
		}

		public Guid UserToken => User != null ? User.Token : Guid.Empty;

		public IUser User
		{
			get
			{
				if (_user == null && Identity != null)
				{
					if (Identity is Identity id)
						_user = id.User;
				}

				return _user;
			}
		}

		public string JwToken
		{
			get
			{
				if (_jw == null)
				{
					if (Shell.HttpContext == null)
						return null;

					if (!Shell.HttpContext.Request.Cookies.ContainsKey(SecurityUtils.AuthenticationCookieName))
						return null;

					var cookie = Shell.HttpContext.Request.Cookies[SecurityUtils.AuthenticationCookieName];
					var json = Serializer.Deserialize<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(cookie)));

					_jw = json.Required<string>("jwt");
				}

				return _jw;
			}
		}

		public ITenant Tenant
		{
			get
			{
				if (_tenant == null)
				{
					var id = Identity as Identity;

					if (id == null || string.IsNullOrWhiteSpace(id.Endpoint))
					{
						if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
							_tenant = Shell.GetService<IConnectivityService>().SelectDefaultTenant();
						else
							return null;
					}
					else
						_tenant = Shell.GetService<IConnectivityService>().SelectTenant(id.Endpoint);
				}

				return _tenant;
			}
		}

		public string RouteUrl(IActionContextProvider provider, string routeName, object values)
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrCannotResolveHttpRequest);

			var svc = Shell.HttpContext.RequestServices.GetService(typeof(IUrlHelperFactory)) as IUrlHelperFactory;

			if (svc == null)
				throw new RuntimeException(SR.ErrNoUrlHelper);

			var helper = svc.GetUrlHelper(provider.ActionContext);

			return helper.RouteUrl(routeName, values);
		}

		public static IMiddlewareDescriptor Current
		{
			get
			{
				return new MiddlewareDescriptor();
			}
		}
	}
}
