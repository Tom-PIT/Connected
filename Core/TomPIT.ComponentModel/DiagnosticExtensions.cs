using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT
{
	public static class DiagnosticExtensions
	{
		public static void LogInfo(this ISysConnection connection, string source)
		{
			LogInfo(connection, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogInfo(this ISysConnection connection, IExecutionContext context, string source)
		{
			LogInfo(connection, context, string.Empty, source, string.Empty, 0);
		}

		public static void LogInfo(this ISysConnection connection, IExecutionContext context, string source, string message)
		{
			LogInfo(connection, context, string.Empty, source, message, 0);
		}

		public static void LogInfo(this ISysConnection connection, IExecutionContext context, string category, string source, string message)
		{
			LogInfo(connection, context, category, source, message, 0);
		}

		public static void LogInfo(this ISysConnection connection, IExecutionContext context, string category, string source, string message, int eventId)
		{
			Write(connection, TraceLevel.Info, context, category, source, message, eventId);
		}

		public static void LogError(this ISysConnection connection, string source)
		{
			LogError(connection, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogError(this ISysConnection connection, IExecutionContext context, string source)
		{
			LogError(connection, context, string.Empty, source, string.Empty, 0);
		}

		public static void LogError(this ISysConnection connection, IExecutionContext context, string source, string message)
		{
			LogError(connection, context, string.Empty, source, message, 0);
		}

		public static void LogError(this ISysConnection connection, string category, string source, string message)
		{
			Write(connection, TraceLevel.Error, null, category, source, message, 0);
		}

		public static void LogError(this ISysConnection connection, IExecutionContext context, string category, string source, string message)
		{
			LogError(connection, context, category, source, message, 0);
		}

		public static void LogError(this ISysConnection connection, IExecutionContext context, string category, string source, string message, int eventId)
		{
			Write(connection, TraceLevel.Error, context, category, source, message, eventId);
		}

		public static void LogWarning(this ISysConnection connection, string source)
		{
			LogWarning(connection, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogWarning(this ISysConnection connection, IExecutionContext context, string source)
		{
			LogWarning(connection, context, string.Empty, source, string.Empty, 0);
		}

		public static void LogWarning(this ISysConnection connection, IExecutionContext context, string source, string message)
		{
			LogWarning(connection, context, string.Empty, source, message, 0);
		}

		public static void LogWarning(this ISysConnection connection, IExecutionContext context, string category, string source, string message)
		{
			LogWarning(connection, context, category, source, message, 0);
		}

		public static void LogWarning(this ISysConnection connection, IExecutionContext context, string category, string source, string message, int eventId)
		{
			Write(connection, TraceLevel.Warning, context, category, source, message, eventId);
		}

		public static void LogVerbose(this ISysConnection connection, string source)
		{
			LogVerbose(connection, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogVerbose(this ISysConnection connection, IExecutionContext context, string source)
		{
			LogVerbose(connection, context, string.Empty, source, string.Empty, 0);
		}

		public static void LogVerbose(this ISysConnection connection, IExecutionContext context, string source, string message)
		{
			LogVerbose(connection, context, string.Empty, source, message, 0);
		}

		public static void LogVerbose(this ISysConnection connection, IExecutionContext context, string category, string source, string message)
		{
			LogVerbose(connection, context, category, source, message, 0);
		}

		public static void LogVerbose(this ISysConnection connection, IExecutionContext context, string category, string source, string message, int eventId)
		{
			Write(connection, TraceLevel.Verbose, context, category, source, message, eventId);
		}

		private static void Write(ISysConnection connection, TraceLevel level, IExecutionContext context, string category, string source, string message, int eventId)
		{
			var svc = connection.GetService<ILoggingService>();

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

		public static RuntimeException WithMetrics(this RuntimeException ex, IExecutionContext context)
		{
			if (context is ExecutionContext ec)
				ex.Metric = ((ContextDiagnosticService)ec.Services.Diagnostic).MetricParent;

			return ex;
		}

		public static JObject ParseRequest(this IMetricConfiguration configuration, HttpRequest request)
		{
			return ParseRequest(configuration, request, null);
		}

		public static JObject ParseRequest(this IMetricConfiguration configuration, HttpRequest request, JObject body)
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

		public static JObject ParseResponse(this IMetricConfiguration configuration, HttpResponse response)
		{
			return ParseResponse(configuration, response, null);
		}

		public static JObject ParseResponse(this IMetricConfiguration configuration, HttpResponse response, string body)
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
