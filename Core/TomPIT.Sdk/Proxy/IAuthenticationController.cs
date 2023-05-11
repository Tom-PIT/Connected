using TomPIT.Security;

namespace TomPIT.Proxy
{
	public interface IAuthenticationController
	{
		IClientAuthenticationResult Authenticate(string user, string password);
		IClientAuthenticationResult AuthenticateByPin(string user, string pin);
	}
}
