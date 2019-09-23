namespace TomPIT.Ide.TextEditor.Languages
{
	public interface ILocation
	{
		IRange Range { get; }
		string Uri { get; }
	}
}
