using System.Net;

namespace TomPIT.Cdn.Dns
{
	internal class A6Record : IRecordData
	{
		public A6Record(DataBuffer buffer)
		{
			Length = buffer.ReadByte();

			if (Length == 0)
				Address = buffer.ReadIPv6Address();
			else if (Length == 128)
				Domain = buffer.ReadDomainName();
			else
			{
				Address = buffer.ReadIPv6Address();
				Domain = buffer.ReadDomainName();
			}
		}

		public int Length { get; } = -1;
		public IPAddress Address { get; } = null;
		public string Domain { get; } = string.Empty;
	}
}
