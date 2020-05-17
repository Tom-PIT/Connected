namespace TomPIT.Ide.VersionControl
{
	public class VersionControlDiffDescriptor : IVersionControlDiffDescriptor
	{
		public string Original { get; set; }

		public string Modified { get; set; }

		public string Syntax { get; set; }
	}
}
