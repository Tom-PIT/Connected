using TomPIT.Security;

namespace TomPIT.Marketplace
{
	internal class AuthenticationResult : IAuthenticationResult
	{
		public string Token { get; set; }
		public bool Success { get; set; }
		public AuthenticationResultReason Reason { get; set; }
	}
}
