namespace TomPIT.Security
{
	internal class AuthorizationSchema : IAuthorizationSchema
	{
		public EmptyBehavior Empty { get; set; } = EmptyBehavior.Deny;
		public AuthorizationLevel Level { get; set; } = AuthorizationLevel.Pessimistic;
	}
}
