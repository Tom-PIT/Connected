namespace TomPIT.Middleware
{
	internal class ElevatedClaim
	{
		public ElevatedClaim(IMiddlewareObject owner, string claim)
		{
			Owner = owner;
			Claim = claim;
		}
		public IMiddlewareObject Owner { get; }
		public string Claim { get; }
	}
}
