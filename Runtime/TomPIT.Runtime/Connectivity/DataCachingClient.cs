﻿using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Caching;
using TomPIT.Notifications;

namespace TomPIT.Connectivity
{
	internal class DataCachingClient : HubClient
	{
		public DataCachingClient(ISysConnection connection, string authenticationToken) : base(connection, authenticationToken)
		{
		}

		protected override string HubName => "datacaching";

		protected override void Initialize()
		{
			Data();
		}

		private void Data()
		{
			Hub.On<MessageEventArgs<DataCacheEventArgs>>("Clear", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IDataCachingService>() is IDataCachingNotification n)
					n.NotifyClear(e.Args);
			});

			Hub.On<MessageEventArgs<DataCacheEventArgs>>("Invalidate", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IDataCachingService>() is IDataCachingNotification n)
					n.NotifyInvalidate(e.Args);
			});

			Hub.On<MessageEventArgs<DataCacheEventArgs>>("Remove", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IDataCachingService>() is IDataCachingNotification n)
					n.NotifyRemove(e.Args);
			});
		}
	}
}