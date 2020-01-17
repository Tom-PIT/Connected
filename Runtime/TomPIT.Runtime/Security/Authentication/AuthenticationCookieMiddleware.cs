using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.Security.Authentication
{
	public class AuthenticationCookieMiddleware
	{
		private readonly RequestDelegate _next;

		public AuthenticationCookieMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			SlideCookie(context);

			await _next(context);
		}

		private void SlideCookie(HttpContext context)
		{
			var cookie = context.Request.Cookies[SecurityUtils.AuthenticationCookieName];

			if (string.IsNullOrWhiteSpace(cookie))
				return;

			var json = Serializer.Deserialize<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(cookie)));

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

				context.Response.Cookies.Delete(SecurityUtils.AuthenticationCookieName);

				context.Response.Cookies.Append(SecurityUtils.AuthenticationCookieName,
					Convert.ToBase64String(Encoding.UTF8.GetBytes(Serializer.Serialize(json))), new CookieOptions
					{
						HttpOnly = true,
						Expires = expires
					}
			);
			}
		}
	}
}
