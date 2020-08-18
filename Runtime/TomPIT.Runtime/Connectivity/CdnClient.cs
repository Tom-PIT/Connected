using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using TomPIT.Cdn;
using TomPIT.Environment;
using TomPIT.Middleware;

namespace TomPIT.Connectivity
{
	internal class CdnClient : ClientConnection
	{
		public CdnClient(IMiddlewareContext context) : base(context.Tenant)
		{
			Context = context;
		}

		protected override string Url => $"{Context.Services.Routing.GetServer(InstanceType.Cdn, InstanceVerbs.Get)}";
		private IMiddlewareContext Context { get; }

		protected override string HubName => "events";

		public void Subscribe(List<IEventHubSubscription> events)
		{
			Hub.InvokeAsync("Add", events).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public void Unsubscribe(List<IEventHubSubscription> events)
		{
			Hub.InvokeAsync("Remove", events).ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}
}
