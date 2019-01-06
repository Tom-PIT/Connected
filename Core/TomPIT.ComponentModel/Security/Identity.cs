using System.Security.Claims;

namespace TomPIT.Security
{
	public class Identity : ClaimsIdentity
	{
		private bool _isAuthenticated = true;

		public Identity(IUser user) : this(user, null, null)
		{
		}

		public Identity(IUser user, string jwToken, string endpoint)
		{
			User = user;
			Token = jwToken;
			Endpoint = endpoint;
			Name = user.AuthenticationToken.ToString();
		}

		public override string AuthenticationType => "Tom PIT";
		public override bool IsAuthenticated { get { return _isAuthenticated; } }
		public override string Name { get; }
		public string Token { get; }

		public string Endpoint { get; }
		public IUser User { get; }

		public static Identity NotAuthenticated()
		{
			return new Identity(null, null, null)
			{
				_isAuthenticated = false
			};
		}
	}
}
