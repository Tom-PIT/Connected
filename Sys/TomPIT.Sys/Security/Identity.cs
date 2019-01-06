using System.Security.Claims;
using TomPIT.Security;

namespace TomPIT.Sys.Security
{
	internal class Identity : ClaimsIdentity
	{
		private bool _isAuthenticated = true;

		public Identity(IUser user)
		{
			User = user;
			Name = user.LoginName;
		}

		public override string AuthenticationType => "Tom PIT";
		public override bool IsAuthenticated { get { return _isAuthenticated; } }
		public override string Name { get; }
		public IUser User { get; }

		public static Identity NotAuthenticated()
		{
			return new Identity(null)
			{
				_isAuthenticated = false
			};
		}
	}
}
