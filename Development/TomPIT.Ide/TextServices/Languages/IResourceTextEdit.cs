namespace TomPIT.Ide.TextServices.Languages
{
	public interface IResourceTextEdit : IResourceEdit
	{
		ITextEdit Edit { get; }
		int ModelVersionId { get; }
		string Resource { get; }
		IWorkspaceEditMetadata Metadata { get; }
	}
}
