namespace TomPIT.Cdn.Dns
{
	internal class MrRecord : DomainNameOnly
	{
		public MrRecord(DataBuffer buffer) : base(buffer) { }
		public string ForwardingAddress { get { return Domain; } }
	}
}
