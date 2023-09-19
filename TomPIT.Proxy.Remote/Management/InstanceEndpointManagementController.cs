using System;
using TomPIT.Environment;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management;
internal class InstanceEndpointManagementController : IInstanceEndpointManagementController
{
    private const string Controller = "InstanceEndpointManagement";
    public void Delete(Guid token)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
        {
            token
        });
    }

    public Guid Insert(InstanceFeatures type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
    {
        return Connection.Post<Guid>(Connection.CreateUrl(Controller, "Insert"), new
        {
            name,
            type,
            url,
            reverseProxyUrl,
            status,
            verbs
        });
    }

    public void Update(Guid token, InstanceFeatures type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Update"), new
        {
            token,
            name,
            type,
            url,
            reverseProxyUrl,
            status,
            verbs
        });
    }
}
