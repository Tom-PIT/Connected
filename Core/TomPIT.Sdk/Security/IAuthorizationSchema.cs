namespace TomPIT.Security
{
	public enum EmptyBehavior
	{
		Deny = 1,
		Alow = 2
	}

	public enum AuthorizationLevel
	{
		Pessimistic = 1,
		Optimistic = 2
	}

	public interface IAuthorizationSchema
	{
		EmptyBehavior Empty { get; set; }
		AuthorizationLevel Level { get; set; }
	}
}
