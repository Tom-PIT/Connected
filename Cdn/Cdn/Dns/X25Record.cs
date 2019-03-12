namespace TomPIT.Cdn.Dns
{
	internal class X25Record : TextOnly
	{
		public X25Record(DataBuffer buffer) : base(buffer) { }
		public string PsdnAddress { get { return Text; } }
	}
}
