using System;

namespace TomPIT.Cdn.Dns
{
	internal class SrvRecord : IRecordData
	{
		private int _priority = 0;
		private ushort _weight = 0;
		private ushort _port = 0;
		private string _domain = string.Empty;

		public SrvRecord(DataBuffer buffer)
		{
			_priority = buffer.ReadShortInt();
			_weight = buffer.ReadShortUInt();
			_port = buffer.ReadShortUInt();
			_domain = buffer.ReadDomainName();
		}
		public override string ToString()
		{
			return String.Format("Priority:{0} Weight:{1}  Port:{2} Domain:{3}", _priority, _weight, _port, _domain);
		}

		public int Priority { get { return _priority; } }
		public ushort Weight { get { return _weight; } }
		public ushort Port { get { return _port; } }
		public string Domain { get { return _domain; } }
	}
}
