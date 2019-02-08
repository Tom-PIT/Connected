using System.Collections.Generic;
using System.Security.Claims;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.IoT.Security
{
	public class DeviceIdentity : ClaimsIdentity
	{
		private List<Claim> _claims = null;

		public DeviceIdentity()
		{

		}

		public DeviceIdentity(IIoTDevice device)
		{
			Device = device;
			Name = Device.Id.ToString();
			Token = device.AuthenticationToken;
		}

		public override IEnumerable<Claim> Claims
		{
			get
			{
				if (_claims == null)
				{
					_claims = new List<Claim>
					{
						new Claim(ClaimTypes.NameIdentifier, Device.Id.ToString()),
						new Claim(ClaimTypes.Name, Device.Name)
					};
				}

				return _claims;
			}
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
