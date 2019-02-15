namespace TomPIT.Connectivity
{
	public interface IBearerCredentials : ICredentials
	{
		string Token { get; }
	}
}
