namespace TomPIT.Compilation;
internal sealed class PrecompilationDiagnostic
{
	public required string FileName { get; set; }
	public int Line { get; set; }
	public required string Message { get; set; }
}
