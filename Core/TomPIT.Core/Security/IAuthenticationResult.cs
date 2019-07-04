namespace TomPIT.Security
{
	public enum AuthenticationResultReason
	{
		OK = 0,
		NotFound = 1,
		InvalidPassword = 2,
		Inactive = 3,
		Locked = 4,
		NoPassword = 5,
		PasswordExpired = 6,
		InvalidToken = 7,
		InvalidCredentials = 8,
		Other = 99
	}

	public interface IAuthenticationResult
	{
		string Token { get; }
		bool Success { get; }
		AuthenticationResultReason Reason { get; }
	}
}
