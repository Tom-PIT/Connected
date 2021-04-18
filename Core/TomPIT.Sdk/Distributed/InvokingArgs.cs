using System;

namespace TomPIT.Distributed
{
	public class DistributedEventInvokingArgs
	{
		public EventInvokingResult Result { get; set; } = EventInvokingResult.Continue;
		public TimeSpan Delay { get; set; }
	}
}
