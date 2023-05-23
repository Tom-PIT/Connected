using System;
using TomPIT.Environment;

namespace TomPIT.Proxy.Management
{
    public interface IInstanceEndpointManagementController
    {
        Guid Insert(InstanceFeatures type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs);
        void Update(Guid token, InstanceFeatures type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs);
        void Delete(Guid token);
    }
}
