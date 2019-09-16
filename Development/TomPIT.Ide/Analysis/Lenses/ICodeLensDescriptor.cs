namespace TomPIT.Ide.Analysis.Lenses
{
	public interface ICodeLensDescriptor
	{
		IRange Range { get; }
		string Id { get; }
		ICodeLensCommand Command { get; }
	}
}
