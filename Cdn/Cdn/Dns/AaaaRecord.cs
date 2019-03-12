using System.Net;

namespace TomPIT.Cdn.Dns
{
	internal class AaaaRecord : IRecordData
	{
		public AaaaRecord(DataBuffer buffer)
		{
			Address = buffer.ReadIPv6Address();
		}

		public IPAddress Address { get; } = null;
	}
}

