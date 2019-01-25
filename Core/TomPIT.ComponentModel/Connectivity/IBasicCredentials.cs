namespace TomPIT.Connectivity
{
	public interface IBasicCredentials : ICredentials
	{
		string UserName { get; }
		string Password { get; }
	}
}
