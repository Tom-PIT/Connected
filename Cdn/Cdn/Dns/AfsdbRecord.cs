namespace TomPIT.Cdn.Dns
{
	internal class AfsdbRecord : IRecordData
	{
		public AfsdbRecord(DataBuffer buffer)
		{
			SubType = buffer.ReadShortInt();
			Domain = buffer.ReadDomainName();
		}

		public short SubType { get; } = 0;
		public string Domain { get; } = string.Empty;
	}
}
