using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	public class CancelEventArguments : DataModelContext
	{
		public CancelEventArguments(IExecutionContext sender) : base(sender)
		{
		}

		public bool Cancel { get; set; }
	}
}
