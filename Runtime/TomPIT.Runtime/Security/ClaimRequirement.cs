using Microsoft.AspNetCore.Authorization;

namespace TomPIT.Security
{
	public class ClaimRequirement : IAuthorizationRequirement
	{
		public ClaimRequirement(string claim)
		{
			Claim = claim;
		}

		public string Claim { get; }
	}
}
