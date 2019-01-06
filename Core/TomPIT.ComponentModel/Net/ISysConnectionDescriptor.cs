namespace TomPIT.Net
{
	public interface ISysConnectionDescriptor
	{
		string Name { get; }
		string Url { get; }
		string ClientKey { get; }
	}
}
