namespace TomPIT.Cdn.Dns
{
	internal class DNameRecord : DomainNameOnly
	{
		public DNameRecord(DataBuffer buffer) : base(buffer) { }

		public string DomainName
		{
			get
			{
				return Domain;
			}
		}
	}
}
