using System;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareDiagnosticService
	{
		void Error(string source, string message, string category);
		void Warning(string source, string message, string category);
		void Info(string source, string message, string category);

		void Console(string message);

		Guid StartMetric(IMetricOptions metric, object args);
		Guid StartMetric(IMetricOptions metric, Guid element, object args);
		void StopMetric(Guid metricId, SessionResult result, object args);

		RuntimeException Exception(string message);
		RuntimeException Exception(string format, string message);
		RuntimeException Exception(string message, int eventId);
		RuntimeException Exception(string format, string message, int eventId);
	}
}
