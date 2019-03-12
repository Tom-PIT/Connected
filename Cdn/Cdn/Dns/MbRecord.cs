namespace TomPIT.Cdn.Dns
{
	internal class MbRecord : DomainNameOnly
	{
		public MbRecord(DataBuffer buffer) : base(buffer) { }
		public string AdminMailboxDomain { get { return Domain; } }
	}
}
