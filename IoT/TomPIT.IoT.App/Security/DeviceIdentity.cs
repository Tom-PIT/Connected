using System.Security.Claims;

namespace TomPIT.IoT.Security
{
	public class DeviceIdentity : ClaimsIdentity
	{
		public DeviceIdentity()
		{

		}

		public DeviceIdentity(IIoTDevice device)
		{
			Device = device;
		}

		public IIoTDevice Device
		{
			get;
		}
		private bool _isAuthenticated = true;
		public override string AuthenticationType => "Tom PIT";
		public override bool IsAuthenticated { get { return _isAuthenticated; } }
		public override string Name { get; }
		public string Token { get; }

		public string Endpoint { get; }

		public static DeviceIdentity NotAuthenticated()
		{
			return new DeviceIdentity()//null, null, null)
			{
				_isAuthenticated = false
			};
		}
	}
}
