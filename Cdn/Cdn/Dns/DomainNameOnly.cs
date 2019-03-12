namespace TomPIT.Cdn.Dns
{
	internal class DomainNameOnly : IRecordData
	{
		private string _domain = string.Empty;

		public DomainNameOnly(DataBuffer buffer)
		{
			_domain = buffer.ReadDomainName();
		}
		public string Domain { get { return _domain; } }

		public override string ToString()
		{
			return "Domain: " + _domain;
		}
	}
}
