namespace TomPIT.Design.Ide
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
