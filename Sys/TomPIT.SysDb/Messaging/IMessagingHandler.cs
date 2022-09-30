namespace TomPIT.SysDb.Messaging
{
	public interface IMessagingHandler
	{
		IQueueHandler Queue { get; }
	}
}
