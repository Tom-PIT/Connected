using System;
using System.Collections.Concurrent;
using System.Net;
using TomPIT.Diagnostics;

namespace TomPIT.Services.Context
{
	internal class ContextDiagnosticService : ContextClient, IContextDiagnosticService
	{
		private Lazy<ConcurrentStack<MetricSession>> _metric = new Lazy<ConcurrentStack<MetricSession>>();

		public ContextDiagnosticService(IExecutionContext context) : base(context)
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

		public void Error(string category, string source, string message)
		{
			Write(new LogEntry
			{
				Category = category,
				Source = source,
				Message = message,
				Level = System.Diagnostics.TraceLevel.Error
			});
		}

		public void Info(string category, string source, string message)
		{
			Write(new LogEntry
			{
				Category = category,
				Source = source,
				Message = message,
				Level = System.Diagnostics.TraceLevel.Info
			});
		}

		public void Warning(string category, string source, string message)
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
			Context.Connection().GetService<ILoggingService>().Write(entry);
		}

		public Guid EnterMetric(Guid component)
		{
			return EnterMetric(component, Guid.Empty);
		}

		public Guid EnterMetric(Guid component, Guid element)
		{
			var id = Guid.NewGuid();



			//var e = Context.Connection().GetService<IDiscoveryService>().Find(component, element);

			Metric.Push(new MetricSession
			{
				Component = component,
				Element = element,
				Session = id,
				Instance = Context.Connection().GetService<IRuntimeService>().Type,
				IP = Shell.HttpContext == null ? IPAddress.None : Shell.HttpContext.Connection.RemoteIpAddress,
				Start = DateTime.UtcNow
			});

			return id;
		}

		public void ExitMetric()
		{
			throw new NotImplementedException();
		}

		public void ExitMetric(Guid id)
		{
			throw new NotImplementedException();
		}

		private ConcurrentStack<MetricSession> Metric { get { return _metric.Value; } }
	}
}
