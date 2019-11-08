using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Management.Distributed
{
	public class ScheduledJobDescriptor
	{
		public IScheduledJob Job { get; set; }
		public string Title { get; set; }

		public IMiddlewareContext Context { get; set; }
	}
}
