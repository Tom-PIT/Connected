using System;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace TomPIT.Cdn.Events
{
	internal class EventMessage
	{
		private static ulong _identity = 0L;

		public EventMessage()
		{
			Id = Interlocked.Increment(ref _identity);
			Expire = DateTime.UtcNow.AddMinutes(1);
		}
		public ulong Id { get; }
		public string Connection { get; set; }
		public string Event { get; set; }
		public string Recipient { get; set; }
		public JObject Arguments { get; set; }
		public int DequeueCount { get; set; } = 1;

		public DateTime NextVisible { get; set; } = DateTime.UtcNow.AddSeconds(5);
		public DateTime Expire { get; }
	}
}
