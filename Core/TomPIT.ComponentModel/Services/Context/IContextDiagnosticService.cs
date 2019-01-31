using Newtonsoft.Json.Linq;
using System;
using TomPIT.ComponentModel;

namespace TomPIT.Services.Context
{
	public interface IContextDiagnosticService
	{
		void Error(string category, string source, string message);
		void Warning(string category, string source, string message);
		void Info(string category, string source, string message);

		void Console(string message);

		Guid StartMetric(IMetricConfiguration metric, JObject request);
		Guid StartMetric(IMetricConfiguration metric, Guid element, JObject request);
		void StopMetric(Guid metricId, JObject response);
	}
}
