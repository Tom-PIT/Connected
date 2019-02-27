using Microsoft.AspNetCore.Authorization;

namespace TomPIT.Runtime.Security
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
