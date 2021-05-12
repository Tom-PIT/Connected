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

			var duration = TimeSpan.FromMinutes(20);
			var durationSetting = json.Optional("renewDuration", 0);

			if (durationSetting > 0)
				duration = TimeSpan.FromSeconds(durationSetting);

			if (dt < DateTime.UtcNow.AddSeconds(duration.TotalSeconds / 2))
			{
				var expires = DateTimeOffset.UtcNow.Add(duration);

				json["expiration"] = expires.Ticks;

				if (durationSetting > 0)
					json["renewDuration"] = durationSetting;

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
