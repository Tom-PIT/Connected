using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Text;

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

			return JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(cookie)));
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
			var body = new StreamReader(s, Encoding.UTF8).ReadToEnd();

			if (string.IsNullOrWhiteSpace(body))
				return new JObject();

			var settings = new JsonSerializerSettings
			{
				Culture = CultureInfo.InvariantCulture,
				DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
				Formatting = Formatting.None,
				NullValueHandling = NullValueHandling.Ignore
			};

			return JsonConvert.DeserializeObject(body, settings) as JObject;
		}

		public static T ToType<T>(this Stream s)
		{
			var body = new StreamReader(s, Encoding.UTF8).ReadToEnd();

			var settings = new JsonSerializerSettings
			{
				Culture = CultureInfo.InvariantCulture,
				DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
				Formatting = Formatting.None,
				NullValueHandling = NullValueHandling.Ignore
			};

			return JsonConvert.DeserializeObject<T>(body, settings);
		}
	}
}
