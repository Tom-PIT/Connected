using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
{
	internal class MessagingHandler : IMessagingHandler
	{
		private IReliableMessagingHandler _messaging;
		private IQueueHandler _queue;

		public IReliableMessagingHandler ReliableMessaging => _messaging ??= new ReliableMessagingHandler();

		public IQueueHandler Queue => _queue ??= new QueueHandler();
	}
}
