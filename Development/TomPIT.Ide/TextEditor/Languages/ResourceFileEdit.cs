namespace TomPIT.Ide.TextEditor.Languages
{
	public class ResourceFileEdit : ResourceEdit, IResourceFileEdit
	{
		public string NewUri { get; set; }

		public string OldUri { get; set; }

		public IResourceFileEditOptions Options { get; set; }
	}
}
