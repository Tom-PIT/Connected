using System.Security.Claims;
using System.Security.Principal;

namespace TomPIT.Sys.Security
{
	internal class Principal : ClaimsPrincipal
	{
		private Identity _identity = null;

		public Principal(Identity identity)
		{
			_identity = identity;

			AddIdentity(_identity);
		}

		public override IIdentity Identity { get { return _identity; } }
	}
}
