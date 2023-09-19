using System;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	internal class Subscriber : ISubscriber
	{
		public string Topic { get; set; }

		public string Connection { get; set; }

		public DateTime Alive { get; set; }

		public DateTime Created { get; set; }

		public Guid Instance { get; set; }

		public long Id { get; set; }
	}
}
