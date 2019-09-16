using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.Security.Authentication
{
	public class AuthenticationCookieFilter : IResultFilter
	{
		public void OnResultExecuted(ResultExecutedContext context)
		{
		}

		public void OnResultExecuting(ResultExecutingContext context)
		{
			var cookie = context.HttpContext.Request.Cookies[SecurityUtils.AuthenticationCookieName];

			if (string.IsNullOrWhiteSpace(cookie))
				return;

			var json = SerializationExtensions.Deserialize<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(cookie)));

			var expiration = json.Optional("expiration", 0L);

			if (expiration == 0)
				return;

			var dt = new DateTime(expiration);

			if (dt < DateTime.UtcNow)
				return;

			if (dt < DateTime.UtcNow.AddMinutes(10))
			{
				var expires = DateTimeOffset.UtcNow.AddMinutes(20);

				json["expiration"] = expires.Ticks;

				context.HttpContext.Response.Cookies.Delete(SecurityUtils.AuthenticationCookieName);

				context.HttpContext.Response.Cookies.Append(SecurityUtils.AuthenticationCookieName,
					Convert.ToBase64String(Encoding.UTF8.GetBytes(SerializationExtensions.Serialize(json))), new CookieOptions
					{
						HttpOnly = true,
						Expires = expires
					}
			);
			}
		}
	}
}
