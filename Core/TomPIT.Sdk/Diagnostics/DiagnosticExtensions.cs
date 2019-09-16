using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Middleware.Services;

namespace TomPIT.Diagostics
{
	public static class DiagnosticExtensions
	{
		public static void LogInfo(this ITenant tenant, string source)
		{
			LogInfo(tenant, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogInfo(this ITenant tenant, IMiddlewareContext context, string source)
		{
			LogInfo(tenant, context, string.Empty, source, string.Empty, 0);
		}

		public static void LogInfo(this ITenant tenant, IMiddlewareContext context, string source, string message)
		{
			LogInfo(tenant, context, string.Empty, source, message, 0);
		}

		public static void LogInfo(this ITenant tenant, IMiddlewareContext context, string category, string source, string message)
		{
			LogInfo(tenant, context, category, source, message, 0);
		}

		public static void LogInfo(this ITenant tenant, IMiddlewareContext context, string category, string source, string message, int eventId)
		{
			Write(tenant, TraceLevel.Info, context, category, source, message, eventId);
		}

		public static void LogError(this ITenant tenant, string source)
		{
			LogError(tenant, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogError(this ITenant tenant, IMiddlewareContext context, string source)
		{
			LogError(tenant, context, string.Empty, source, string.Empty, 0);
		}

		public static void LogError(this ITenant tenant, IMiddlewareContext context, string source, string message)
		{
			LogError(tenant, context, string.Empty, source, message, 0);
		}

		public static void LogError(this ITenant tenant, string category, string source, string message)
		{
			Write(tenant, TraceLevel.Error, null, category, source, message, 0);
		}

		public static void LogError(this ITenant tenant, IMiddlewareContext context, string category, string source, string message)
		{
			LogError(tenant, context, category, source, message, 0);
		}

		public static void LogError(this ITenant tenant, IMiddlewareContext context, string category, string source, string message, int eventId)
		{
			Write(tenant, TraceLevel.Error, context, category, source, message, eventId);
		}

		public static void LogWarning(this ITenant tenant, string source)
		{
			LogWarning(tenant, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogWarning(this ITenant tenant, IMiddlewareContext context, string source)
		{
			LogWarning(tenant, context, string.Empty, source, string.Empty, 0);
		}

		public static void LogWarning(this ITenant tenant, IMiddlewareContext context, string source, string message)
		{
			LogWarning(tenant, context, string.Empty, source, message, 0);
		}

		public static void LogWarning(this ITenant tenant, IMiddlewareContext context, string category, string source, string message)
		{
			LogWarning(tenant, context, category, source, message, 0);
		}

		public static void LogWarning(this ITenant tenant, IMiddlewareContext context, string category, string source, string message, int eventId)
		{
			Write(tenant, TraceLevel.Warning, context, category, source, message, eventId);
		}

		public static void LogVerbose(this ITenant tenant, string source)
		{
			LogVerbose(tenant, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogVerbose(this ITenant tenant, IMiddlewareContext context, string source)
		{
			LogVerbose(tenant, context, string.Empty, source, string.Empty, 0);
		}

		public static void LogVerbose(this ITenant tenant, IMiddlewareContext context, string source, string message)
		{
			LogVerbose(tenant, context, string.Empty, source, message, 0);
		}

		public static void LogVerbose(this ITenant tenant, IMiddlewareContext context, string category, string source, string message)
		{
			LogVerbose(tenant, context, category, source, message, 0);
		}

		public static void LogVerbose(this ITenant tenant, IMiddlewareContext context, string category, string source, string message, int eventId)
		{
			Write(tenant, TraceLevel.Verbose, context, category, source, message, eventId);
		}

		private static void Write(ITenant tenant, TraceLevel level, IMiddlewareContext context, string category, string source, string message, int eventId)
		{
			var svc = tenant.GetService<ILoggingService>();

			var e = new LogEntry
			{
				Category = category,
				EventId = eventId,
				Level = level,
				Message = message,
				Source = source,
			};

			svc.Write(e);
		}

		public static RuntimeException WithMetrics(this RuntimeException ex, IMiddlewareContext context)
		{
			if (context is MiddlewareContext mw)
				ex.Metric = ((MiddlewareDiagnosticService)mw.Services.Diagnostic).MetricParent;

			return ex;
		}

		public static JObject ParseRequest(this IMetricOptions configuration, HttpRequest request)
		{
			return ParseRequest(configuration, request, null);
		}

		public static JObject ParseRequest(this IMetricOptions configuration, HttpRequest request, JObject body)
		{
			if (!configuration.Enabled || configuration.Level != MetricLevel.Detail)
				return null;

			var r = new JObject();

			var metaData = new JObject();

			r.Add("metaData", metaData);

			metaData.Add("contentLength", request.ContentLength);
			metaData.Add("contentType", request.ContentType);
			metaData.Add("host", request.Host.ToString());
			metaData.Add("ajax", request.IsAjaxRequest());
			metaData.Add("method", request.Method);
			metaData.Add("path", request.Path.ToString());
			metaData.Add("pathBase", request.PathBase.ToString());
			metaData.Add("protocol", request.Protocol);
			metaData.Add("queryString", request.QueryString.ToString());
			metaData.Add("scheme", request.Scheme);

			var u = request.HttpContext.User.Identity.IsAuthenticated
				? request.HttpContext.User.Identity.Name
				: "anonimous";

			metaData.Add("user", u);
			metaData.Add("localIpAddress", request.HttpContext.Connection.LocalIpAddress.ToString());
			metaData.Add("localPort", request.HttpContext.Connection.LocalPort.ToString());
			metaData.Add("remoteIpAddress", request.HttpContext.Connection.RemoteIpAddress.ToString());
			metaData.Add("remotePort", request.HttpContext.Connection.RemotePort.ToString());

			var headers = new JObject();

			r.Add("headers", headers);

			foreach (var i in request.Headers)
				headers.Add(i.Key, i.Value.ToString());

			if (body != null)
				r.Add("body", body);

			return r;
		}

		public static JObject ParseResponse(this IMetricOptions configuration, HttpResponse response)
		{
			return ParseResponse(configuration, response, null);
		}

		public static JObject ParseResponse(this IMetricOptions configuration, HttpResponse response, string body)
		{
			if (!configuration.Enabled || configuration.Level != MetricLevel.Detail)
				return null;

			var r = new JObject();

			var metaData = new JObject();

			r.Add("metaData", metaData);

			metaData.Add("contentLength", response.ContentLength);
			metaData.Add("contentType", response.ContentType);
			metaData.Add("statusCode", response.StatusCode);

			var headers = new JObject();

			r.Add("headers", headers);

			foreach (var i in response.Headers)
				headers.Add(i.Key, i.Value.ToString());

			if (!string.IsNullOrWhiteSpace(body))
				r.Add("body", body);

			return r;
		}
	}
}
