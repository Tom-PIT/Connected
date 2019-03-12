using System;

namespace TomPIT.Cdn.Dns
{
	internal class RpRecord : IRecordData
	{
		private string _responsibleMailbox = string.Empty;
		private string _textDomain = string.Empty;

		public RpRecord(DataBuffer buffer)
		{
			_responsibleMailbox = buffer.ReadDomainName();
			_textDomain = buffer.ReadDomainName();
		}
		public override string ToString()
		{
			return String.Format("Responsible Mailbox:{0} Text Domain:{1}", _responsibleMailbox, _textDomain);
		}

		public string ResponsibleMailbox { get { return _responsibleMailbox; } }
		public string TextDomain { get { return _textDomain; } }
	}
}
