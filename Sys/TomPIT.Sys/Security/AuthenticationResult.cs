using TomPIT.Security;

namespace TomPIT.Sys.Security
{
	internal class AuthenticationResult : IAuthenticationResult
	{
		public string Token { get; set; }
		public bool Success { get; set; }
		public AuthenticationResultReason Reason { get; set; }

		public static IAuthenticationResult Fail(AuthenticationResultReason reason)
		{
			return new AuthenticationResult
			{
				Success = false,
				Reason = reason
			};
		}

		public static IAuthenticationResult OK(string token)
		{
			return new AuthenticationResult
			{
				Success = true,
				Token = token
			};
		}
	}
}
