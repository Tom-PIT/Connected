using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Middleware.Services;
using TomPIT.Reflection;

namespace TomPIT.Diagnostics
{
	public static class DiagnosticExtensions
	{
		public static void LogInfo(this ITenant tenant, string source)
		{
			LogInfo(tenant, source, null);
		}

		public static void LogInfo(this ITenant tenant, string source, string message)
		{
			LogInfo(tenant, source, message, null);
		}

		public static void LogInfo(this ITenant tenant, string source, string message, string category)
		{
			LogInfo(tenant, source, message, category, 0);
		}

		public static void LogInfo(this ITenant tenant, string source, string message, string category, int eventId)
		{
			Write(tenant, TraceLevel.Info, source, message, category, eventId);
		}

		public static void LogError(this ITenant tenant, string source)
		{
			LogError(tenant, source, null);
		}

		public static void LogError(this ITenant tenant, string source, string message)
		{
			LogError(tenant, source, message, null);
		}

		public static void LogError(this ITenant tenant, string source, string message, string category)
		{
			LogError(tenant, source, message, category, 0);
		}

		public static void LogError(this ITenant tenant, string source, string message, string category, int eventId)
		{
			Write(tenant, TraceLevel.Error, source, message, category, eventId);
		}

		public static void LogWarning(this ITenant tenant, string source)
		{
			LogWarning(tenant, source, null);
		}

		public static void LogWarning(this ITenant tenant, string source, string message)
		{
			LogWarning(tenant, source, message, null);
		}

		public static void LogWarning(this ITenant tenant, string source, string message, string category)
		{
			LogWarning(tenant, source, message, category, 0);
		}

		public static void LogWarning(this ITenant tenant, string source, string message, string category, int eventId)
		{
			Write(tenant, TraceLevel.Warning, source, message, category, eventId);
		}

		public static void LogVerbose(this ITenant tenant, string source)
		{
			LogVerbose(tenant, source, null);
		}

		public static void LogVerbose(this ITenant tenant, string source, string message)
		{
			LogVerbose(tenant, source, message, null);
		}

		public static void LogVerbose(this ITenant tenant, string source, string message, string category)
		{
			LogVerbose(tenant, source, message, category, 0);
		}

		public static void LogVerbose(this ITenant tenant, string source, string message, string category, int eventId)
		{
			Write(tenant, TraceLevel.Verbose, source, message, category, eventId);
		}

		private static void Write(ITenant tenant, TraceLevel level, string source, string message, string category, int eventId)
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

		public static ExceptionTraceDescriptor ParseDescriptor(this DiagnosticDescriptor descriptor, IMiddlewareContext context)
		{
			if (context is null || descriptor is null || descriptor.Method is null)
				return null;

			if (descriptor.Method.DeclaringType is not Type type)
				return null;

			var component = context.Tenant.GetService<ICompilerService>().ResolveComponent(type);

			if (component is null)
				return null;

			var ms = context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);
			var config = context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token);
			var types = context.Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);

			var result = new ExceptionTraceDescriptor
			{
				Component = component.Name,
				MicroService = ms.Name,
				Line = descriptor.Line
			};

			foreach (var script in types)
			{
				if (string.Compare(Path.GetFileNameWithoutExtension(script.FileName), type.Name, false) == 0)
				{
					result.FileName = script.FileName;

					var devUrl = context.Tenant.GetService<IInstanceEndpointService>().Url(InstanceType.Development, InstanceVerbs.All);

					if (!string.IsNullOrWhiteSpace(devUrl))
						result.Url = $"{devUrl}/ide/{ms.Url}?component={component.Token}&element={script.Id}";

					var text = context.Tenant.GetService<IComponentService>().SelectText(ms.Token, script);

					if (!string.IsNullOrWhiteSpace(text))
					{
						using var reader = new StringReader(text);
						var index = 1;

						while (reader.Peek() != -1)
						{
							var line = reader.ReadLine();

							if (IsInScope(descriptor.Line, index))
								result.SourceCodeLines.Add(index, line);

							index++;
							
							if (result.SourceCodeLines.Count == 7)
								break;
						}
					}

					break;
				}
			}

			return result;
		}

		private static bool IsInScope(int lineNumber, int index)
		{
			if (lineNumber < 7)
				return index <= 7;

			return index >= lineNumber - 3 && index <= lineNumber + 3;
		}
	}
}
