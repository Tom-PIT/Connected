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
		public int PermissionCount { get; set; }

		public static AuthorizationResult OK(int permissionCount)
		{
			return new AuthorizationResult
			{
				Success = true,
				Reason = AuthorizationResultReason.OK,
				PermissionCount = permissionCount
			};
		}

		public static AuthorizationResult Fail(AuthorizationResultReason reason, int permissionCount)
		{
			return new AuthorizationResult
			{
				Success = false,
				Reason = reason,
				PermissionCount = permissionCount
			};
		}
	}
}
