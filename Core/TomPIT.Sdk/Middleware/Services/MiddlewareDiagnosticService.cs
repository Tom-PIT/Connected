using System;
using System.Collections.Concurrent;
using System.Net;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Serialization;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareDiagnosticService : MiddlewareObject, IMiddlewareDiagnosticService
	{
		private Lazy<ConcurrentDictionary<Guid, Metric>> _metric = new Lazy<ConcurrentDictionary<Guid, Metric>>();

		public MiddlewareDiagnosticService(IMiddlewareContext context) : base(context)
		{
		}

		public Guid MetricParent { get; set; }

		public void Console(string message)
		{
			Write(new LogEntry
			{
				Category = "Console",
				Source = "Output",
				Message = message,
				Level = System.Diagnostics.TraceLevel.Verbose,
				Metric = MetricParent
			});

		}

		public void Error(string source, string message, string category)
		{
			Write(new LogEntry
			{
				Category = category,
				Source = source,
				Message = message,
				Level = System.Diagnostics.TraceLevel.Error,
				Metric = MetricParent
			});
		}

		public void Info(string source, string message, string category)
		{
			Write(new LogEntry
			{
				Category = category,
				Source = source,
				Message = message,
				Level = System.Diagnostics.TraceLevel.Info,
				Metric = MetricParent
			});
		}

		public void Warning(string source, string message, string category)
		{
			Write(new LogEntry
			{
				Category = category,
				Source = source,
				Message = message,
				Level = System.Diagnostics.TraceLevel.Warning,
				Metric = MetricParent
			});
		}

		private void Write(ILogEntry entry)
		{
			if (entry is LogEntry e)
				e.Metric = MetricParent;

			Context.Tenant.GetService<ILoggingService>().Write(entry);
		}

		public Guid StartMetric(IMetricOptions metric, object args)
		{
			return StartMetric(metric, Guid.Empty, args);
		}

		public Guid StartMetric(IMetricOptions metric, Guid element, object args)
		{
			if (!metric.Enabled)
				return Guid.Empty;

			var content = args == null ? string.Empty : Serializer.Serialize(args);
			var length = content.Length;
			var id = Guid.NewGuid();

			Metric.TryAdd(id, new Metric
			{
				Component = metric.Configuration().Component,
				Element = element,
				Session = id,
				Instance = Shell.GetService<IRuntimeService>().Type,
				IP = Shell.HttpContext == null ? IPAddress.None.ToString() : Shell.HttpContext.Connection.RemoteIpAddress.ToString(),
				Start = DateTime.UtcNow,
				Request = content,
				ConsumptionIn = length,
				Parent = MetricParent
			});

			MetricParent = id;

			return id;
		}

		public void StopMetric(Guid metricId, SessionResult result, object args)
		{
			if (metricId == Guid.Empty)
				return;

			var content = args == null ? string.Empty : Serializer.Serialize(args);
			var length = content.Length;

			if (Metric.TryRemove(metricId, out Metric m))
			{
				m.ConsumptionOut = length;
				m.Response = content;
				m.End = DateTime.UtcNow;
				m.Result = result;
				MetricParent = m.Session;

				Context.Tenant.GetService<IMetricService>().Write(m);
			}
		}

		private ConcurrentDictionary<Guid, Metric> Metric { get { return _metric.Value; } }

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
	}
}
