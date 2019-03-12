namespace TomPIT.Cdn.Dns
{
	internal class NullRecord : TextOnly
	{
		public NullRecord(DataBuffer buffer, int length) : base(buffer, length) { }
	}
}