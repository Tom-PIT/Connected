using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Security.Claims;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT.Routing
{
	public abstract class RouteHandlerBase
	{
		private ISysConnection _connection = null;

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

		protected ISysConnection Connection
		{
			get
			{
				if (_connection == null)
				{
					var endpoint = Context.Request.GetAuthenticationEndpoint();

					if (endpoint == null)
						return null;

					_connection = Shell.GetService<IConnectivityService>().Select(endpoint);
				}

				return _connection;
			}
		}

		protected IUser User
		{
			get
			{
				if (Context.User == null)
					return null;

				var claim = Context.User.FindFirst(f => string.Compare(f.Type, ClaimTypes.NameIdentifier, true) == 0);

				if (claim == null)
					return null;

				if (!Guid.TryParse(claim.Value, out Guid at))
					return null;

				return Connection.GetService<IUserService>().SelectByAuthenticationToken(at);
			}
		}
	}
}
