using System;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	internal class MessageRecipient : IRecipient
	{
		public string Connection { get; set; }

		public Guid Message { get; set; }

		public string Content { get; set; }

		public string Topic { get; set; }

		public int RetryCount { get; set; }
		public DateTime NextVisible { get; set; }

		public long Id { get; set; }
	}
}
