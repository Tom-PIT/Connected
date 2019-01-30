using TomPIT.SysDb.Diagnostics;

namespace TomPIT.SysDb.Sql.Diagnostics
{
	internal class DiagnosticHandler : IDiagnosticHandler
	{
		private ILoggingHandler _logging = null;
		private IMetricHandler _metric = null;

		public ILoggingHandler Logging
		{
			get
			{
				if (_logging == null)
					_logging = new LoggingHandler();

				return _logging;
			}
		}

		public IMetricHandler Metrics
		{
			get
			{
				if (_metric == null)
					_metric = new MetricsHandler();

				return _metric;
			}
		}
	}
}
