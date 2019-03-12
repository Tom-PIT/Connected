using System.Net;

namespace TomPIT.Cdn.Dns
{
	internal class ARecord : IRecordData
	{
		public ARecord(DataBuffer buffer)
		{
			Address = new IPAddress(buffer.ReadBytes(4));
		}

		public IPAddress Address { get; } = null;
	}
}
