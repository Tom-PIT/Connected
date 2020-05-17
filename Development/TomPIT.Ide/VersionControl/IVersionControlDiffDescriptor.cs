namespace TomPIT.Ide.VersionControl
{
	public interface IVersionControlDiffDescriptor
	{
		string Original { get; }
		string Modified { get; }
		string Syntax { get; }
	}
}
