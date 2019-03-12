namespace TomPIT.Cdn.Dns
{
	internal class NsRecord : DomainNameOnly
	{
		public NsRecord(DataBuffer buffer) : base(buffer) { }
		public string NsDomain { get { return Domain; } }
	}
}
