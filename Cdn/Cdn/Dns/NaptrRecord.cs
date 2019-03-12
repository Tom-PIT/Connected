using System;

namespace TomPIT.Cdn.Dns
{
	internal class NaptrRecord : IRecordData
	{
		private ushort _order = 0;
		private ushort _priority = 0;
		private string _flags = string.Empty;
		private string _services = string.Empty;
		private string _regexp = string.Empty;
		private string _replacement = string.Empty;

		public NaptrRecord(DataBuffer buffer)
		{
			_order = buffer.ReadShortUInt();
			_priority = buffer.ReadShortUInt();
			_flags = buffer.ReadCharString();
			_services = buffer.ReadCharString();
			_regexp = buffer.ReadCharString();
			_replacement = buffer.ReadCharString();
		}
		public override string ToString()
		{
			return String.Format("Order:{0}, Priority:{1} Flags:{2} Services:{3} RegExp:{4} Replacement:{5}",
				 _order, _priority, _flags, _services, _regexp, _replacement);
		}

		public ushort Order { get { return _order; } }
		public ushort Priority { get { return _priority; } }
		public string Flags { get { return _flags; } }
		public string Services { get { return _services; } }
		public string Regexp { get { return _regexp; } }
		public string Replacement { get { return _replacement; } }
	}
}
