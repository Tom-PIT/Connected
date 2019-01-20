namespace TomPIT.Services
{
	public interface IClientSysConnection
	{
		string Name { get; }
		string Url { get; }
		string AuthenticationToken { get; }
	}
}
