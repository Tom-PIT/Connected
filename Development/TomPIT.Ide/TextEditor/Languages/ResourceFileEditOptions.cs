namespace TomPIT.Ide.TextEditor.Languages
{
	public class ResourceFileEditOptions : IResourceFileEditOptions
	{
		public bool Overwrite { get; set; }

		public bool IgnoreIfNotExists { get; set; }

		public bool IgnoreIfExists { get; set; }

		public bool Recursive { get; set; }
	}
}
