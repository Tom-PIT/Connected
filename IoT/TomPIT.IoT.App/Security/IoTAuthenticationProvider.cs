using TomPIT.Security;

namespace TomPIT.IoT.Security
{
	public class IoTAuthenticationProvider : IAuthenticationProvider
	{
		public IClientAuthenticationResult Authenticate(string userName, string password)
		{
			return null;
		}

		public IClientAuthenticationResult Authenticate(string bearerKey)
		{
			if (string.Compare(bearerKey, "test", true) == 0)
			{
				return new AuthenticationResult
				{
					Reason = AuthenticationResultReason.OK,
					Success = true,
					Identity = new DeviceIdentity()
				};
			}
			else
			{
				return new AuthenticationResult
				{
					Reason = AuthenticationResultReason.NotFound,
					Success = false,
					Identity = DeviceIdentity.NotAuthenticated()
				};
			}
		}
	}
}
