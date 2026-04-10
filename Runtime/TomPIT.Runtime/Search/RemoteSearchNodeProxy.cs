using System;
using TomPIT.ComponentModel.Search;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Runtime;

namespace TomPIT.Search
{
    internal class RemoteSearchNodeProxy : TenantObject, ISearchNodeProxy
    {
        public RemoteSearchNodeProxy(ITenant tenant) : base(tenant)
        {
        }

        public ISearchResults Search(ISearchOptions options)
        {
            var url = Tenant.GetService<IInstanceEndpointService>().Url(InstanceFeatures.Search, InstanceVerbs.Post);

            if (string.IsNullOrWhiteSpace(url))
                throw new RuntimeException($"{SR.ErrNoServer} ({InstanceFeatures.Search}, {InstanceVerbs.Post})");

            var u = ServerUrl.Create(url, "Search", "Search");
            var args = new HttpRequestArgs().WithCurrentCredentials(MiddlewareDescriptor.Current.User == null ? Guid.Empty : MiddlewareDescriptor.Current.User.AuthenticationToken);

            return Tenant.Post<SearchResults>(u, options, args);
        }
    }
}
