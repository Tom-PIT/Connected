using System.Diagnostics;
using TomPIT.Diagnostics;
using TomPIT.Middleware;

namespace TomPIT
{
	public static class Tenant
	{
		public static T GetService<T>()
		{
			return MiddlewareDescriptor.Current.Tenant.GetService<T>();
		}

		public static void LogInfo(string source)
		{
			LogInfo(source, null);
		}

		public static void LogInfo(string source, string message)
		{
			LogInfo(source, message, null);
		}

		public static void LogInfo(string source, string message, string category)
		{
			LogInfo(source, message, category, 0);
		}

		public static void LogInfo(string source, string message, string category, int eventId)
		{
			Write(TraceLevel.Info, source, message, category, eventId);
		}

		public static void LogError(string source)
		{
			LogError(source, null);
		}

		public static void LogError(string source, string message)
		{
			LogError(source, message, null);
		}

		public static void LogError(string source, string message, string category)
		{
			LogError(source, message, category, 0);
		}

		public static void LogError(string source, string message, string category, int eventId)
		{
			Write(TraceLevel.Error, source, message, category, eventId);
		}

		public static void LogWarning(string source)
		{
			LogWarning(source, null);
		}

		public static void LogWarning(string source, string message)
		{
			LogWarning(source, message, null);
		}

		public static void LogWarning(string source, string message, string category)
		{
			LogWarning(source, message, category, 0);
		}

		public static void LogWarning(string source, string message, string category, int eventId)
		{
			Write(TraceLevel.Warning, source, message, category, eventId);
		}

		public static void LogVerbose(string source)
		{
			LogVerbose(source, null);
		}

		public static void LogVerbose(string source, string message)
		{
			LogVerbose(source, message, null);
		}

		public static void LogVerbose(string source, string message, string category)
		{
			LogVerbose(source, message, category, 0);
		}

		public static void LogVerbose(string source, string message, string category, int eventId)
		{
			Write(TraceLevel.Verbose, source, message, category, eventId);
		}

		private static void Write(TraceLevel level, string source, string message, string category, int eventId)
		{
			var svc = GetService<ILoggingService>();

			var e = new LogEntry
			{
				Category = category,
				EventId = eventId,
				Level = level,
				Message = message,
				Source = source,
			};

			svc.Write(e);

			try {
				System.Console.WriteLine($"{level}\t{source}\t{message}\t{category}\t{eventId}");
			}
			catch{ }
		}

	}
}
