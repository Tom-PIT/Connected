namespace TomPIT.Security
{
	public enum AuthorizationResultReason
	{
		OK = 0,
		Empty = 1,
		NoPrimaryKey = 2,
		NoAllowFound = 3,
		DenyFound = 4,
		NoClaim = 5,
		Other = 99
	}

	public interface IAuthorizationResult
	{
		bool Success { get; }
		AuthorizationResultReason Reason { get; }
	}
}
