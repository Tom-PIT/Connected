using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT.Routing
{
	public abstract class RouteHandlerBase
	{
		private ITenant _tenant = null;

		public void ProcessRequest(HttpContext context)
		{
			Context = context;

			OnProcessRequest();
		}

		protected virtual void OnProcessRequest()
		{

		}

		protected HttpContext Context { get; private set; }

		protected bool HasBeenModified(DateTime date)
		{
			date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);

			if (!string.IsNullOrEmpty(Context.Request.Headers["If-Modified-Since"]))
			{
				var provider = CultureInfo.InvariantCulture;
				var lastMod = DateTime.ParseExact(Context.Request.Headers["If-Modified-Since"], "r", provider).ToLocalTime();

				if (lastMod == date)
				{
					Context.Response.StatusCode = 304;

					return false;
				}
			}

			return true;
		}

		protected void SetModified(DateTime date)
		{
			date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);

			Context.Response.Headers["Last-Modified"] = date.ToUniversalTime().ToString("r");
		}

		protected ITenant Tenant
		{
			get
			{
				if (_tenant == null)
				{
					var endpoint = Context.Request.GetAuthenticationEndpoint();

					if (endpoint == null)
						return null;

					_tenant = Shell.GetService<IConnectivityService>().SelectTenant(endpoint);
				}

				return _tenant;
			}
		}

		protected IUser User
		{
			get
			{
				if (Context.User == null)
					return null;

				if (!(Context.User.Identity is Identity id) || id.User == null || id.User.AuthenticationToken == Guid.Empty)
					return null;

				return Tenant.GetService<IUserService>().SelectByAuthenticationToken(id.User.AuthenticationToken);
			}
		}
	}
}
