namespace TomPIT.Runtime.Configuration
{
	public interface IConnectionConfiguration
	{
		string Name { get; }
		string Url { get; }
		string AuthenticationToken { get; }
	}
}
