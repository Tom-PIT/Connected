using System;
using System.Diagnostics;
using System.IO;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Middleware;
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

					var devUrl = context.Tenant.GetService<IInstanceEndpointService>().Url(InstanceFeatures.Development, InstanceVerbs.All);

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
