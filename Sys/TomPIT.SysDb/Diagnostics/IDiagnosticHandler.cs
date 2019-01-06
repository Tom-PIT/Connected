namespace TomPIT.SysDb.Diagnostics
{
	public interface IDiagnosticHandler
	{
		ILoggingHandler Logging { get; }
	}
}
