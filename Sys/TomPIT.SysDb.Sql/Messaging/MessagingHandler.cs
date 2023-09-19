using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
{
	internal class MessagingHandler : IMessagingHandler
	{
		private IQueueHandler _queue;

		public IQueueHandler Queue => _queue ??= new QueueHandler();
	}
}
