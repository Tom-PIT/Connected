using System.Diagnostics;
using TomPIT.Diagnostics;
using TomPIT.Net;
using TomPIT.Runtime;

namespace TomPIT
{
	public static class DiagnosticExtensions
	{
		public static void LogInfo(this ISysContext context, string source)
		{
			LogInfo(context, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogInfo(this ISysContext context, IApplicationContext appContext, string source)
		{
			LogInfo(context, appContext, string.Empty, source, string.Empty, 0);
		}

		public static void LogInfo(this ISysContext context, IApplicationContext appContext, string source, string message)
		{
			LogInfo(context, appContext, string.Empty, source, message, 0);
		}

		public static void LogInfo(this ISysContext context, IApplicationContext appContext, string category, string source, string message)
		{
			LogInfo(context, appContext, category, source, message, 0);
		}

		public static void LogInfo(this ISysContext context, IApplicationContext appContext, string category, string source, string message, int eventId)
		{
			Write(context, TraceLevel.Info, appContext, category, source, message, eventId);
		}

		public static void LogError(this ISysContext context, string source)
		{
			LogError(context, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogError(this ISysContext context, IApplicationContext appContext, string source)
		{
			LogError(context, appContext, string.Empty, source, string.Empty, 0);
		}

		public static void LogError(this ISysContext context, IApplicationContext appContext, string source, string message)
		{
			LogError(context, appContext, string.Empty, source, message, 0);
		}

		public static void LogError(this ISysContext context, IApplicationContext appContext, string category, string source, string message)
		{
			LogError(context, appContext, category, source, message, 0);
		}

		public static void LogError(this ISysContext context, IApplicationContext appContext, string category, string source, string message, int eventId)
		{
			Write(context, TraceLevel.Error, appContext, category, source, message, eventId);
		}

		public static void LogWarning(this ISysContext context, string source)
		{
			LogWarning(context, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogWarning(this ISysContext context, IApplicationContext appContext, string source)
		{
			LogWarning(context, appContext, string.Empty, source, string.Empty, 0);
		}

		public static void LogWarning(this ISysContext context, IApplicationContext appContext, string source, string message)
		{
			LogWarning(context, appContext, string.Empty, source, message, 0);
		}

		public static void LogWarning(this ISysContext context, IApplicationContext appContext, string category, string source, string message)
		{
			LogWarning(context, appContext, category, source, message, 0);
		}

		public static void LogWarning(this ISysContext context, IApplicationContext appContext, string category, string source, string message, int eventId)
		{
			Write(context, TraceLevel.Warning, appContext, category, source, message, eventId);
		}

		public static void LogVerbose(this ISysContext context, string source)
		{
			LogVerbose(context, null, string.Empty, source, string.Empty, 0);
		}

		public static void LogVerbose(this ISysContext context, IApplicationContext appContext, string source)
		{
			LogVerbose(context, appContext, string.Empty, source, string.Empty, 0);
		}

		public static void LogVerbose(this ISysContext context, IApplicationContext appContext, string source, string message)
		{
			LogVerbose(context, appContext, string.Empty, source, message, 0);
		}

		public static void LogVerbose(this ISysContext context, IApplicationContext appContext, string category, string source, string message)
		{
			LogVerbose(context, appContext, category, source, message, 0);
		}

		public static void LogVerbose(this ISysContext context, IApplicationContext appContext, string category, string source, string message, int eventId)
		{
			Write(context, TraceLevel.Verbose, appContext, category, source, message, eventId);
		}

		private static void Write(ISysContext context, TraceLevel level, IApplicationContext appContext, string category, string source, string message, int eventId)
		{
			var svc = context.GetService<ILoggingService>();

			var e = new LogEntry
			{
				Authority = appContext == null ? string.Empty : appContext.Identity.Authority,
				AuthorityId = appContext == null ? string.Empty : appContext.Identity.AuthorityId,
				Category = category,
				EventId = eventId,
				Level = TraceLevel.Error,
				Message = message,
				Source = source,
				MicroService = appContext.MicroService(),
			};

			svc.Write(e);
		}
	}
}
