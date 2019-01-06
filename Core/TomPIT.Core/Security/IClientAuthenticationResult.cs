using System.Security.Claims;

namespace TomPIT.Security
{
	public interface IClientAuthenticationResult : IAuthenticationResult
	{
		ClaimsIdentity Identity { get; }
	}
}
