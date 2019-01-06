namespace TomPIT.Security
{
	internal class AuthorizationSchema : IAuthorizationSchema
	{
		public EmptyBehavior Empty { get; set; }

		public AuthorizationLevel Level { get; set; }
	}
}
