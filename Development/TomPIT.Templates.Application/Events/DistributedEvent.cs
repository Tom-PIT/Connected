using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Application.Events
{
	public class DistributedEvent : ComponentConfiguration, IDistributedEvent
	{
		public const string ComponentCategory = "Event";
	}
}
