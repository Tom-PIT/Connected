using TomPIT.Runtime;

namespace TomPIT.ComponentModel
{
	public class CancelEventArguments : EventArguments
	{
		public CancelEventArguments(IApplicationContext sender) : base(sender)
		{
		}

		public bool Cancel { get; set; }
	}
}
