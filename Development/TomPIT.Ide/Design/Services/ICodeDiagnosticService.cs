namespace TomPIT.Design.Services
{
	public interface ICodeDiagnosticService
	{
		ICodeDiagnosticProvider GetProvider(string language);
	}
}
