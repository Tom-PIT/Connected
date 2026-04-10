using Newtonsoft.Json.Linq;
using TomPIT.Cdn.Clients;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.Cdn
{
    internal class ClientNotificationProxyService : ICdnClientNotificationProxy
    {
        public void Notify(string token, string method, object arguments)
        {
            var client = MiddlewareDescriptor.Current.Tenant.GetService<IClientService>().Select(token);

            if (client == null)
                throw new NotFoundException(SR.ErrClientNotFound);

            if (ClientHubs.Clients == null)
                return;

            ClientHubs.Clients.Clients.Group(token.ToLowerInvariant())
                .SendCoreAsync("message", new object[] { method, token, arguments is JObject jo ? jo : arguments });
        }
    }
}
