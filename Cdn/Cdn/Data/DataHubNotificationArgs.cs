using System;

namespace TomPIT.Cdn.Data
{
	public class DataHubNotificationArgs : EventArgs
	{
		public DataHubNotificationArgs(string eventName, string arguments)
		{
			Name = eventName;
			Arguments = arguments;
		}

		public string Name { get; }
		public string Arguments { get; }
	}
}
