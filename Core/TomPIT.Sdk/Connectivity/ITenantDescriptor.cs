namespace TomPIT.Connectivity
{
	public interface ITenantDescriptor
	{
		string Name { get; }
		string Url { get; }
		string ClientKey { get; }
	}
}
