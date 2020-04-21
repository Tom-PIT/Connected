namespace TomPIT.Ide.TextServices
{
	public interface ITextModel
	{
		string Id { get; }
		string Uri { get; }
		int Version { get; }
	}
}
