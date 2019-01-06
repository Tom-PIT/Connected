using TomPIT.Diagnostics;

namespace TomPIT.Runtime.ApplicationContextServices
{
	internal class LoggingService : ILoggingService
	{
		public LoggingService(IApplicationContext context)
		{
			Context = context;
		}

		private IApplicationContext Context { get; }

		public void Console(string message)
		{
			Write(new LogEntry
			{
				Category = "Console",
				Source = "Output",
				Message = message,
				Level = System.Diagnostics.TraceLevel.Verbose,
				MicroService = Context.MicroService()
			});

		}

		public void Error(string category, string source, string message)
		{
			Write(new LogEntry
			{
				Category = category,
				Source = source,
				Message = message,
				Level = System.Diagnostics.TraceLevel.Error,
				MicroService = Context.MicroService()
			});
		}

		public void Info(string category, string source, string message)
		{
			Write(new LogEntry
			{
				Category = category,
				Source = source,
				Message = message,
				Level = System.Diagnostics.TraceLevel.Info,
				MicroService = Context.MicroService()
			});
		}

		public void Warning(string category, string source, string message)
		{
			Write(new LogEntry
			{
				Category = category,
				Source = source,
				Message = message,
				Level = System.Diagnostics.TraceLevel.Warning,
				MicroService = Context.MicroService()
			});
		}

		private void Write(ILogEntry entry)
		{
			Context.GetServerContext().GetService<Diagnostics.ILoggingService>().Write(entry);
		}
	}
}
