using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	public class CancelEventArguments : EventArguments
	{
		public CancelEventArguments(IExecutionContext sender) : base(sender)
		{
		}

		public bool Cancel { get; set; }
	}
}
