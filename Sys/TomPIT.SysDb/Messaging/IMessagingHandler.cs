namespace TomPIT.SysDb.Messaging
{
	public interface IMessagingHandler
	{
		IReliableMessagingHandler ReliableMessaging { get; }
		IQueueHandler Queue { get; }
	}
}
