using System;
using System.Diagnostics;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Services;

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
			//LogError(connection, context, category, source, message, 0);
		}

		public static void LogError(this ISysConnection connection, string category, string source, string message, Guid component, Guid element)
		{
			//LogError(connection, context, category, source, message, 0);
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
				Authority = context == null ? string.Empty : context.Identity.Authority,
				AuthorityId = context == null ? string.Empty : context.Identity.AuthorityId,
				Category = category,
				EventId = eventId,
				Level = TraceLevel.Error,
				Message = message,
				Source = source,
				MicroService = context == null ? Guid.Empty : context.MicroService(),
			};

			svc.Write(e);
		}
	}
}
