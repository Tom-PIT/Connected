namespace TomPIT.Ide.ComponentModel
{
	public enum IdeResourceType
	{
		Stylesheet = 1,
		Script = 2
	}
	public interface IIdeResource
	{
		IdeResourceType Type { get; }
		string Path { get; }
	}
}
