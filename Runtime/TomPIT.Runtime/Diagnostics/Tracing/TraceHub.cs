using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Diagnostics.Tracing
{
    [Authorize(AuthenticationSchemes = "TomPIT")]
    public class TraceHub : Hub<ITraceHubClient>
    {
        public TraceHub()
        {
        }

        public static async Task Trace(IHubContext<TraceHub> context, ITraceMessage message)
        {
            await context.Clients.Groups(message.Endpoint.Identifier).SendAsync("OnTrace", message);
        }       

        public async Task Subscribe(TraceEndpoint endpoint)
        {
            await this.Groups.AddToGroupAsync(Context.ConnectionId, (endpoint as ITraceEndpoint).Identifier);
        }

        public async Task Unsubscribe(TraceEndpoint endpoint)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, (endpoint as ITraceEndpoint).Identifier);
        }
    }
}
