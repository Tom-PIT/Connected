using TomPIT.IoT.Services;
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
			var device = Instance.Connection.GetService<IIoTHubService>().SelectDevice(bearerKey);

			if (device != null)
			{
				return new AuthenticationResult
				{
					Reason = AuthenticationResultReason.OK,
					Success = true,
					Identity = new DeviceIdentity(device)
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
