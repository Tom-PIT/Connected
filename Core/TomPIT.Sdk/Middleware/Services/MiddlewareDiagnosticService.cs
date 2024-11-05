using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Reflection;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareDiagnosticService : MiddlewareObject, IMiddlewareDiagnosticService
	{
		public MiddlewareDiagnosticService(IMiddlewareContext context) : base(context)
		{
		}

		public void Console(string message)
		{
			Write(new LogEntry
			{
				Category = "Console",
				Source = "Output",
				Message = message,
				Level = System.Diagnostics.TraceLevel.Verbose
			});

		}

		public void Error(string source, string message, string category)
		{
			Write(new LogEntry
			{
				Category = category,
				Source = source,
				Message = message,
				Level = System.Diagnostics.TraceLevel.Error
			});
		}

		public void Info(string source, string message, string category)
		{
			Write(new LogEntry
			{
				Category = category,
				Source = source,
				Message = message,
				Level = System.Diagnostics.TraceLevel.Info
			});
		}

		public void Warning(string source, string message, string category)
		{
			Write(new LogEntry
			{
				Category = category,
				Source = source,
				Message = message,
				Level = System.Diagnostics.TraceLevel.Warning
			});
		}

		private void Write(ILogEntry entry)
		{
			Context.Tenant.GetService<ILoggingService>().Write(entry);
		}

		public RuntimeException Exception(string message)
		{
			return Exception(message, 0);
		}

		public RuntimeException Exception(string format, string message)
		{
			return Exception(format, message, 0);
		}

		public RuntimeException Exception(string message, int eventId)
		{
			return new RuntimeException(Context.GetType().ShortName(), message)
			{
				Event = eventId
			};
		}

		public RuntimeException Exception(string format, string message, int eventId)
		{
			return new RuntimeException(Context.GetType().ShortName(), string.Format("{0}", message))
			{
				Event = eventId
			};
		}

		public void Dump(string text)
		{
			Context.Tenant.GetService<ILoggingService>().Dump(text);
		}
	}
}
