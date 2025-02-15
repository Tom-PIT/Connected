﻿using System;
using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	public static class Awaiter
	{
		public static void WaitForEvent(IMiddlewareContext context, string eventName, Func<object, bool> handler)
		{
			WaitForEvent(context, eventName, handler, TimeSpan.FromMinutes(1));
		}
		public static void WaitForEvent(IMiddlewareContext context, string eventName, Func<object, bool> handler, TimeSpan timeout)
		{
			var connection = context.Tenant.GetService<ICdnService>().Connect(context);

			connection.Subscribe(new List<IEventHubSubscription>
			{
				new EventHubSubscription
				{
					Name = eventName
				}
			});
		}
	}
}
