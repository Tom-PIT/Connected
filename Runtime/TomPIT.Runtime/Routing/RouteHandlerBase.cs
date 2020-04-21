using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Routing
{
	public abstract class RouteHandlerBase
	{
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
			else if (string.IsNullOrEmpty(Context.Request.Headers["ETag"]))
			{
				var lastMod = new DateTime(Convert.ToInt64(Context.Request.Headers["If-Modified-Since"]));

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
			Context.Response.Headers["ETag"] = date.ToUniversalTime().Ticks.ToString();
			Context.Response.Headers["Cache-Control"] = "public, max-age=600";
		}

		protected ITenant Tenant => MiddlewareDescriptor.Current.Tenant;
		protected IUser User => MiddlewareDescriptor.Current.User;
	}
}
