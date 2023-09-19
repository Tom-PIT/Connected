using System;
using TomPIT.Environment;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class InstanceEndpointManagementController : IInstanceEndpointManagementController
{
    public void Delete(Guid token)
    {
        DataModel.InstanceEndpoints.Delete(token);
    }

    public Guid Insert(InstanceFeatures type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
    {
        return DataModel.InstanceEndpoints.Insert(type, name, url, reverseProxyUrl, status, verbs);
    }

    public void Update(Guid token, InstanceFeatures type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
    {
        DataModel.InstanceEndpoints.Update(token, type, name, url, reverseProxyUrl, status, verbs);
    }
}
