using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Proxy;

namespace TomPIT.DataProviders.BigData
{
    internal class RemoteBigDataProxy : IBigDataProxy
    {
        private readonly string _dataSource;

        public RemoteBigDataProxy(string dataSource)
        {
            _dataSource = dataSource;
        }

        public void Write(IPartitionConfiguration configuration, JArray data)
        {
            var msToken = configuration.MicroService();
            var ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(msToken);
            var u = $"{_dataSource}/data/{ms.Name}/{configuration.ComponentName()}";

            MiddlewareDescriptor.Current.Tenant.Post(u, data);
        }

        public JArray Query(IPartitionConfiguration configuration, JArray parameters)
        {
            var msToken = configuration.MicroService();
            var ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(msToken);
            var u = $"{_dataSource}/query/{ms.Name}/{configuration.ComponentName()}";
            HttpRequestArgs credentialArgs = null;

            if (MiddlewareDescriptor.Current?.Identity?.IsAuthenticated ?? false)
                credentialArgs = new HttpRequestArgs().WithCurrentCredentials(MiddlewareDescriptor.Current.User.AuthenticationToken);

            return MiddlewareDescriptor.Current.Tenant.Post<JArray>(u, parameters, credentialArgs);
        }
    }
}
