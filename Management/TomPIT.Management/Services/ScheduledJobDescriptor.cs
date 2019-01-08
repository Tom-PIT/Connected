namespace TomPIT.Services
{
	public class ScheduledJobDescriptor
	{
		public IScheduledJob Job { get; set; }
		public string Title { get; set; }

		public IExecutionContext Context { get; set; }
	}
}
