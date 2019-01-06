using TomPIT.Runtime;
using TomPIT.Workers;

namespace TomPIT.ComponentModel.Workers
{
	public class ScheduledJobDescriptor
	{
		public IScheduledJob Job { get; set; }
		public string Title { get; set; }

		public IApplicationContext Context { get; set; }
	}
}
