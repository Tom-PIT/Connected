using System.Security.Claims;

namespace TomPIT.IoT.Security
{
	public class DeviceIdentity : ClaimsIdentity
	{
		private bool _isAuthenticated = true;

		//public DeviceIdentity(IUser user) : this(user, null, null)
		//{
		//}

		//public DeviceIdentity(IUser user, string jwToken, string endpoint)
		//{
		//	User = user;
		//	Token = jwToken;
		//	Endpoint = endpoint;
		//	Name = user.AuthenticationToken.ToString();
		//}

		public override string AuthenticationType => "Tom PIT";
		public override bool IsAuthenticated { get { return _isAuthenticated; } }
		public override string Name { get; }
		public string Token { get; }

		public string Endpoint { get; }
		//public IUser User { get; }

		public static DeviceIdentity NotAuthenticated()
		{
			return new DeviceIdentity()//null, null, null)
			{
				_isAuthenticated = false
			};
		}
	}
}
