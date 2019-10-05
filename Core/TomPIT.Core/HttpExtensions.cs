using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT
{
	public static class HttpExtensions
	{
		public static bool IsAjaxRequest(this HttpRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (request.Headers != null && request.Headers.ContainsKey("X-Requested-With"))
				return string.Compare(request.Headers["X-Requested-With"], "XMLHttpRequest", true) == 0;

			return false;
		}

		public static JObject GetAuthenticationCookie(this HttpRequest request)
		{
			if (request == null || !request.Cookies.ContainsKey(SecurityUtils.AuthenticationCookieName))
				return null;

			var cookie = request.Cookies[SecurityUtils.AuthenticationCookieName];

			return Serializer.Deserialize<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(cookie)));
		}

		public static string GetAuthenticationEndpoint(this HttpRequest request)
		{
			var cookie = GetAuthenticationCookie(request);

			if (cookie == null)
				return null;

			return cookie.Required<string>("endpoint");
		}

		public static JObject RequestBody(this Controller controller)
		{
			return controller.Request.Body.ToJObject();
		}

		public static JObject ToJObject(this Stream s)
		{
			using (var reader = new StreamReader(s, Encoding.UTF8))
			{
				var body = reader.ReadToEndAsync().Result;

				if (string.IsNullOrWhiteSpace(body))
					return new JObject();

				return Serializer.Deserialize<JObject>(body);
			}
		}

		public static T ToType<T>(this Stream s)
		{
			var body = new StreamReader(s, Encoding.UTF8).ReadToEndAsync().Result;

			return Serializer.Deserialize<T>(body);
		}
	}
}
