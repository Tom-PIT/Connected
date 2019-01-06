namespace TomPIT.Security
{
	internal class AuthorizationResult : IAuthorizationResult
	{
		public AuthorizationResult()
		{

		}

		public AuthorizationResult(bool success, AuthorizationResultReason reason)
		{

		}

		public bool Success { get; set; }
		public AuthorizationResultReason Reason { get; set; }

		public static AuthorizationResult OK()
		{
			return new AuthorizationResult
			{
				Success = true,
				Reason = AuthorizationResultReason.OK
			};
		}

		public static AuthorizationResult Fail(AuthorizationResultReason reason)
		{
			return new AuthorizationResult
			{
				Success = false,
				Reason = reason
			};
		}
	}
}
