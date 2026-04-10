using System;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Middleware;

namespace TomPIT.Runtime
{
    internal class RemoteDesignNotificationProxy : TenantObject, IDesignNotificationProxy
    {
        private readonly string _url;
        private readonly string _authenticationToken;

        public RemoteDesignNotificationProxy(ITenant tenant, string url, string authenticationToken) : base(tenant)
        {
            _url = url;
            _authenticationToken = authenticationToken;
        }

        public void ConfigurationAdded(Guid component)
        {
            Tenant.Post(CreateUrl("ConfigurationAdded"), new { component }, new HttpRequestArgs().WithBearerCredentials(_authenticationToken));
        }

        public void ConfigurationChanged(Guid component)
        {
            Tenant.Post(CreateUrl("ConfigurationChanged"), new { component }, new HttpRequestArgs().WithBearerCredentials(_authenticationToken));
        }

        public void ConfigurationRemoved(Guid component)
        {
            Tenant.Post(CreateUrl("ConfigurationRemoved"), new { component }, new HttpRequestArgs().WithBearerCredentials(_authenticationToken));
        }

        public void SourceTextChanged(Guid microService, Guid component, Guid token, int type)
        {
            Tenant.Post(CreateUrl("SourceTextChanged"), new { microService, component, token, type }, new HttpRequestArgs().WithBearerCredentials(_authenticationToken));
        }

        private string CreateUrl(string action) => $"{_url}/sys/debug/{action}";
    }
}
