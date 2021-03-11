using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT
{
	public static class HttpExtensions
	{
		public const string HeaderParamPrefix = "X-TP-PARAM-";
		private const string RequestArgumentsKey = "TP-REQUEST-ARGUMENTS";
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
			using var reader = new StreamReader(s, Encoding.UTF8);
			var body = reader.ReadToEndAsync().Result;

			if (string.IsNullOrWhiteSpace(body))
				return new JObject();

			var result = Serializer.Deserialize<JObject>(body);

			SetRequestArguments(Shell.HttpContext, result);

			return result;
		}

		public static T ToType<T>(this Stream s)
		{
			var body = new StreamReader(s, Encoding.UTF8).ReadToEndAsync().Result;

			return Serializer.Deserialize<T>(body);
		}

		public static JObject GetRequestArguments(this HttpContext context)
		{
			var result = context.Items[RequestArgumentsKey];

			return result == null ? null : (JObject)result;
		}

		public static void SetRequestArguments(this HttpContext context, JObject arguments)
		{
			if (context == null)
				return;

			context.Items[RequestArgumentsKey] = arguments;
		}

		public static JObject ParseArguments(this HttpContext context, object staticArguments, Action<string> refererRouteMatcher)
		{
			return ParseArguments(context, staticArguments, null, refererRouteMatcher);
		}
		
		public static JObject ParseArguments(this HttpContext context, object staticArguments)
		{
			return ParseArguments(context, staticArguments, null, null);
		}
		public static JObject ParseArguments(this HttpContext context, object staticArguments, string queryString)
		{
			return ParseArguments(context, staticArguments, queryString, null);
		}
		public static JObject ParseArguments(this HttpContext context, object staticArguments, string queryString, Action<string> refererRouteMatcher)
		{
			var result = staticArguments == null
				? new JObject()
				: staticArguments is JObject ? staticArguments as JObject : Serializer.Deserialize<JObject>(Serializer.Serialize(staticArguments));

			if (!string.IsNullOrWhiteSpace(queryString))
			{
				var query = QueryHelpers.ParseQuery(queryString);

				foreach (var key in query)
				{
					if (!result.ContainsKey(key.Key))
						result.Add(new JProperty(key.Key, key.Value.ToString()));
				}
			}

			if (Shell.HttpContext is not null)
			{
				foreach (var header in Shell.HttpContext.Request.Headers)
				{
					if (string.Compare(header.Key, "Referer", true) == 0 && Shell.HttpContext.Request.IsAjaxRequest())
					{
						var value = header.Value.ToString();

						if (value.Contains("?"))
						{
							var qs = QueryHelpers.ParseQuery(value.Split(new char[] { '?' }, 2)[1]);

							foreach (var k in qs)
							{
								if (!result.ContainsKey(k.Key))
									result.Add(new JProperty(k.Key, k.Value.ToString()));
							}
						}

						if (refererRouteMatcher != null)
							refererRouteMatcher(value);
					}

					if (!header.Key.StartsWith(HeaderParamPrefix, StringComparison.OrdinalIgnoreCase))
						continue;

					var key = header.Key[HeaderParamPrefix.Length..];

					if (!result.ContainsKey(key))
						result.Add(new JProperty(key, header.Value.ToString()));
				}

				var routeData = Shell.HttpContext.GetRouteData();

				if (routeData != null)
				{
					foreach (var value in routeData.Values)
					{
						if (!result.ContainsKey(value.Key))
							result.Add(new JProperty(value.Key, value.Value));
					}
				}
			}

			return result;
		}
	}
}
