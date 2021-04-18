using System;

namespace TomPIT.Cdn.Mail
{
	internal class InboxHeader : IInboxHeader
	{
		public InboxHeader()
		{

		}

		public InboxHeader(string field, string value)
		{
			Field = field;
			Value = value;
		}
		public HeaderId Id
		{
			get
			{
				if (!Enum.IsDefined(typeof(HeaderId), Field))
					return HeaderId.Unknown;

				return (HeaderId)Enum.Parse(typeof(HeaderId), Field);
			}
		}

		public string Field { get; set; }

		public string Value { get; set; }
	}
}
