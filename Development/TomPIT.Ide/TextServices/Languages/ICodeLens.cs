namespace TomPIT.Ide.TextServices.Languages
{
	public interface ICodeLens
	{
		ICommand Command { get; }
		string Id { get; }
		IRange Range { get; }
	}
}
