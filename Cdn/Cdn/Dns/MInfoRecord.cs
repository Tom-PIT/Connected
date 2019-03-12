using System;

namespace TomPIT.Cdn.Dns
{
	internal class MInfoRecord : IRecordData
	{
		private string _responsibleMailbox = string.Empty;
		private string _errorMailbox = string.Empty;

		public MInfoRecord(DataBuffer buffer)
		{
			_responsibleMailbox = buffer.ReadDomainName();
			_errorMailbox = buffer.ReadDomainName();
		}
		public override string ToString()
		{
			return String.Format("Responsible Mailbox:{0} Error Mailbox:{1}", _responsibleMailbox, _errorMailbox);
		}

		public string ResponsibleMailbox { get { return _responsibleMailbox; } }
		public string ErrorMailbox { get { return _errorMailbox; } }
	}
}
