using System;
using Newtonsoft.Json.Linq;

namespace TomPIT.Cdn.Events
{
	public class EventClient
	{
		public string ConnectionId { get; set; }
		public Guid User { get; set; }

		public string EventName { get; set; }
		public JObject Arguments { get; set; }
	}
}
