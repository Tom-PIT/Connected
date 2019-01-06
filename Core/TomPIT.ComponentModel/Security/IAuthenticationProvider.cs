namespace TomPIT.Security
{
	public interface IAuthenticationProvider
	{
		IClientAuthenticationResult Authenticate(string userName, string password);
		IClientAuthenticationResult Authenticate(string bearerKey);
	}
}
