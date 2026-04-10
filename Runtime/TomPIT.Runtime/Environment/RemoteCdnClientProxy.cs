using TomPIT.Cdn;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Security;

namespace TomPIT.Environment
{
    internal class RemoteCdnClientProxy : TenantObject, ICdnClientNotificationProxy
    {
        public RemoteCdnClientProxy(ITenant tenant) : base(tenant)
        {
        }

        public void Notify(string token, string method, object arguments)
        {
            var cdn = Tenant.GetService<IInstanceEndpointService>().Select(InstanceFeatures.Cdn);

            if (cdn == null)
                throw new Exceptions.NotFoundException($"{SR.ErrInstanceEndpointNotFound} ({InstanceFeatures.Cdn})");

            var url = $"{cdn.Url}/sys/clients/notify";
            var provider = Tenant.GetService<IAuthorizationService>() as IAuthenticationTokenProvider;
            var authToken = provider?.RequestToken(InstanceFeatures.Cdn) ?? string.Empty;

            Tenant.Post(url, new { token, method, arguments }, new HttpRequestArgs().WithBearerCredentials(authToken));
        }
    }
}
