namespace TomPIT.Cdn.Dns
{
	internal class MgRecord : DomainNameOnly
	{
		public MgRecord(DataBuffer buffer) : base(buffer) { }
		public string MailGroupDomain { get { return Domain; } }
	}
}
