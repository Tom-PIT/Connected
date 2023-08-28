namespace TomPIT.Ide.TextServices.Languages
{
	public class ResourceTextEdit : ResourceEdit, IResourceTextEdit
	{
		public ITextEdit TextEdit { get; set; }

		public int ModelVersionId { get; set; }

		public string Resource { get; set; }
		public IWorkspaceEditMetadata Metadata { get; set; }
	}
}
