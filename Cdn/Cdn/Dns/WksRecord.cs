using System;
using System.Net;

namespace TomPIT.Cdn.Dns
{
	internal class WksRecord : IRecordData
	{
		private IPAddress _ipAddress = null;
		private byte _protocol = 0;
		private byte[] _services = null;

		public WksRecord(DataBuffer buffer, int length)
		{
			_ipAddress = buffer.ReadIPAddress();
			_protocol = buffer.ReadByte();
			_services = new byte[length - 5];

			for (int i = 0; i < (length - 5); i++)
				_services[i] = buffer.ReadByte();
		}
		public override string ToString()
		{
			return String.Format("IP Address:{0} Protocol:{1} Services:{2}", _ipAddress, _protocol, _services);
		}

		public IPAddress IpAddress { get { return _ipAddress; } }
		public byte Protocol { get { return _protocol; } }
		public byte[] Services { get { return _services; } }
	}
}
