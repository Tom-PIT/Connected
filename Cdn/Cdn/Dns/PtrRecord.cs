namespace TomPIT.Cdn.Dns
{
	internal class PtrRecord : DomainNameOnly
	{
		public PtrRecord(DataBuffer buffer) : base(buffer) { }
		public string PtrDomain { get { return Domain; } }
	}
}
