namespace TomPIT.Ide.TextEditor.Languages
{
	public interface IResourceFileEdit : IResourceEdit
	{
		string NewUri { get; }
		string OldUri { get; }
		IResourceFileEditOptions Options { get; }
	}
}