using System;

namespace TomPIT.Security
{
	public interface IAuthorizationChain
	{
		Guid AuthorizationParent { get; }
	}
}
