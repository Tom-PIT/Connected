using System.Security.Claims;
using TomPIT.Security;

namespace TomPIT.Proxy.Local
{
	internal class AuthenticationResult : IClientAuthenticationResult
	{
		public string Token { get; set; }
		public bool Success { get; set; }
		public AuthenticationResultReason Reason { get; set; }
		public ClaimsIdentity Identity { get; set; }
	}
}
