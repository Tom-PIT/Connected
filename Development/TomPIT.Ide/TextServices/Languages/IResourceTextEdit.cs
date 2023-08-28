namespace TomPIT.Ide.TextServices.Languages
{
    public interface IResourceTextEdit : IResourceEdit
    {
        ITextEdit TextEdit { get; }
        int ModelVersionId { get; }
        string Resource { get; }
        IWorkspaceEditMetadata Metadata { get; }
    }
}
