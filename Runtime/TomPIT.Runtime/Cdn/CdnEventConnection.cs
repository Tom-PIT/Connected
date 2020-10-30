using System;
using System.Collections.Generic;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	internal class CdnEventConnection : ICdnEventConnection
	{
		private CdnClient _client = null;
		private List<IEventHubSubscription> _subscriptions = null;
		public CdnEventConnection(IMiddlewareContext context)
		{
			Context = context;
		}

		public void Connect()
		{
			Client.Connected += OnConnected;
			Client.Connect();
		}

		private void OnConnected(object sender, System.EventArgs e)
		{
			if (Subscriptions.Count > 0)
				Client.Subscribe(Subscriptions);
		}

		public void Subscribe(List<IEventHubSubscription> events)
		{
			Subscriptions.AddRange(events);

			if (Client.IsConnected)
				Client.Subscribe(events);
		}

		public void Unsubscribe(List<IEventHubSubscription> events)
		{
			foreach (var e in events)
				Subscriptions.Remove(e);

			if (Client.IsConnected)
				Client.Unsubscribe(events);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			GC.SuppressFinalize(this);

			if (_client != null)
				_client.Dispose();


			_client = null;
		}
		private IMiddlewareContext Context { get; }
		private CdnClient Client
		{
			get
			{
				if (_client == null)
					_client = new CdnClient(Context);

				return _client;
			}
		}

		private List<IEventHubSubscription> Subscriptions
		{
			get
			{
				if (_subscriptions == null)
					_subscriptions = new List<IEventHubSubscription>();

				return _subscriptions;
			}
		}
	}
}
