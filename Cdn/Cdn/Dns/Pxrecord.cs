namespace TomPIT.Cdn.Dns
{
	internal class PxRecord : PrefAndDomain
	{
		private string _x400Domain = string.Empty;

		public PxRecord(DataBuffer buffer)
			: base(buffer)
		{
			_x400Domain = buffer.ReadDomainName();
		}

		public string X400Domain { get { return _x400Domain; } }
	}
}
