namespace TomPIT.Ide.TextServices.Languages
{
	public class ResourceTextEdit : ResourceEdit, IResourceTextEdit
	{
		public ITextEdit Edit { get; set; }

		public int ModelVersionId { get; set; }

		public string Resource { get; set; }
		public IWorkspaceEditMetadata Metadata { get; set; }
	}
}
