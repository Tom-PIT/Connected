using System;

namespace TomPIT.Security
{
	public interface IAuthenticationProvider
	{
		IClientAuthenticationResult Authenticate(string userName, string password);
		IClientAuthenticationResult Authenticate(string authenticationToken);
		IClientAuthenticationResult Authenticate(Guid authenticationToken);
	}
}
