using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Net;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Diagnostics;

namespace TomPIT.Services.Context
{
	internal class ContextDiagnosticService : ContextClient, IContextDiagnosticService
	{
		private Lazy<ConcurrentDictionary<Guid, MetricSession>> _metric = new Lazy<ConcurrentDictionary<Guid, MetricSession>>();

		public ContextDiagnosticService(IExecutionContext context) : base(context)
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

		public void Error(string category, string source, string message)
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

		public void Info(string category, string source, string message)
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

		public void Warning(string category, string source, string message)
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

			Context.Connection().GetService<ILoggingService>().Write(entry);
		}

		public Guid StartMetric(IMetricConfiguration metric, JObject request)
		{
			return StartMetric(metric, Guid.Empty, request);
		}

		public Guid StartMetric(IMetricConfiguration metric, Guid element, JObject request)
		{
			if (!metric.Enabled)
				return Guid.Empty;

			var content = request == null ? string.Empty : Types.Serialize(request);
			var length = content.Length;
			var id = Guid.NewGuid();

			Metric.TryAdd(id, new MetricSession
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

		public void StopMetric(Guid metricId, SessionResult result, JObject response)
		{
			if (metricId == Guid.Empty)
				return;

			var content = response == null ? string.Empty : Types.Serialize(response);
			var length = content.Length;

			if (Metric.TryRemove(metricId, out MetricSession m))
			{
				m.ConsumptionOut = length;
				m.Response = content;
				m.End = DateTime.UtcNow;
				m.Result = result;
				MetricParent = m.Session;

				Context.Connection().GetService<IMetricService>().Write(m);
			}
		}

		private ConcurrentDictionary<Guid, MetricSession> Metric { get { return _metric.Value; } }
	}
}
