using TomPIT.SysDb.Diagnostics;

namespace TomPIT.SysDb.Sql.Diagnostics
{
	internal class DiagnosticHandler : IDiagnosticHandler
	{
		private ILoggingHandler _logging = null;

		public ILoggingHandler Logging
		{
			get
			{
				if (_logging == null)
					_logging = new LoggingHandler();

				return _logging;
			}
		}
	}
}
