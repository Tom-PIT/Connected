using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
{
	internal class MessagingHandler : IMessagingHandler
	{
		private IReliableMessagingHandler _messaging = null;
		private IQueueHandler _queue = null;

		public IReliableMessagingHandler ReliableMessaging
		{
			get
			{
				if (_messaging == null)
					_messaging = new ReliableMessagingHandler();

				return _messaging;
			}
		}

		public IQueueHandler Queue
		{
			get
			{
				if (_queue == null)
					_queue = new QueueHandler();

				return _queue;
			}
		}
	}
}
