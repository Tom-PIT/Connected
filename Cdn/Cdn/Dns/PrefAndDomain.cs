using System;

namespace TomPIT.Cdn.Dns
{
	internal class PrefAndDomain : IRecordData
	{
		private int _preference = 0;
		private string _domain = string.Empty;

		protected PrefAndDomain() { }
		public PrefAndDomain(DataBuffer buffer)
		{
			_preference = buffer.ReadBEShortInt();
			_domain = buffer.ReadDomainName();
		}

		public int Preference { get { return _preference; } protected set { _preference = value; } }
		public string Domain { get { return _domain; } protected set { _domain = value; } }
		public override string ToString()
		{
			return String.Format("Preference:{0} Domain:{1}", _preference, _domain);
		}
	}
}
