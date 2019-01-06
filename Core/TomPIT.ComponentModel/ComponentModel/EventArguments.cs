using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	public class EventArguments : ExecutionContext
	{
		public EventArguments(IExecutionContext sender) : base(sender)
		{

		}
	}
}
