using System;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	internal class Message : IMessage
	{
		public string Text { get; set; }

		public string Topic { get; set; }

		public DateTime Created { get; set; }

		public DateTime Expire { get; set; }

		public TimeSpan RetryInterval { get; set; }

		public Guid Token { get; set; }

		public long Id { get; set; }
	}
}
