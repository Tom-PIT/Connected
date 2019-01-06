using System.Security.Claims;

namespace TomPIT.Security
{
	public class AuthenticationResult : IClientAuthenticationResult
	{
		public string Token { get; set; }
		public bool Success { get; set; }
		public AuthenticationResultReason Reason { get; set; }
		public ClaimsIdentity Identity { get; set; }
	}
}
