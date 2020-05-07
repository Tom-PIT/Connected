namespace TomPIT.Middleware.Services
{
	internal class MiddlewareCdnService : MiddlewareObject, IMiddlewareCdnService
	{
		private IMiddlewareEmail _email = null;
		private IMiddlewareSubscriptions _subscriptions = null;
		private IMiddlewareDataHub _dataHub = null;
		private IMiddlewareEvents _events = null;
		private IMiddlewareQueue _queue = null;
		private IMiddlewarePrinting _printing = null;
		public MiddlewareCdnService(IMiddlewareContext context) : base(context)
		{
		}

		public IMiddlewareEmail Mail
		{
			get
			{
				if (_email == null)
					_email = new MiddlewareEmail();

				return _email;
			}
		}

		public IMiddlewareSubscriptions Subscriptions
		{
			get
			{
				if (_subscriptions == null)
					_subscriptions = new MiddlewareSubscriptions();

				return _subscriptions;
			}
		}

		public IMiddlewareDataHub DataHub
		{
			get
			{
				if (_dataHub == null)
					_dataHub = new MiddlewareDataHub();

				return _dataHub;
			}
		}

		public IMiddlewareEvents Events
		{
			get
			{
				if (_events == null)
					_events = new MiddlewareEvents();

				return _events;
			}
		}

		public IMiddlewareQueue Queue
		{
			get
			{
				if (_queue == null)
					_queue = new MiddlewareQueue();

				return _queue;
			}
		}

		public IMiddlewarePrinting Printing
		{
			get
			{
				if (_printing == null)
					_printing = new MiddlewarePrinting();

				return _printing;
			}
		}
	}
}
