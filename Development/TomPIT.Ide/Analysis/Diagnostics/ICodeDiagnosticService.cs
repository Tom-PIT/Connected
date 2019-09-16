namespace TomPIT.Ide.Analysis.Diagnostics
{
	public interface ICodeDiagnosticService
	{
		ICodeDiagnosticProvider GetProvider(string language);
	}
}
