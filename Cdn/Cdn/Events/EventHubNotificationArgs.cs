using System;

namespace TomPIT.Cdn.Events
{
	public class EventHubNotificationArgs : EventArgs
	{
		public EventHubNotificationArgs(string eventName, string arguments)
		{
			Name = eventName;
			Arguments = arguments;
		}

		public string Name { get; }
		public string Arguments { get; }
	}
}
