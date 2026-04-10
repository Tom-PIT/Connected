using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TomPIT.BigData.Persistence;
using TomPIT.BigData.Transactions;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;
using TomPIT.Proxy;
using TomPIT.Serialization;

namespace TomPIT.BigData
{
    internal class BigDataProxyService : TenantObject, IBigDataProxy
    {
        public BigDataProxyService(ITenant tenant) : base(tenant)
        {
        }

        public void Write(IPartitionConfiguration configuration, JArray data)
        {
            Tenant.GetService<ITransactionService>().Prepare(configuration, data);
        }

        public JArray Query(IPartitionConfiguration configuration, JArray parameters)
        {
            var queryParams = new List<QueryParameter>();

            foreach (JObject parameter in parameters)
            {
                var property = parameter.First as JProperty;
                var value = ((JValue)property.Value).Value;

                try
                {
                    value = Serializer.Deserialize<JArray>(value);
                }
                catch { }

                queryParams.Add(new QueryParameter { Name = property.Name, Value = value });
            }

            return Tenant.GetService<IPersistenceService>().Query(configuration, queryParams);
        }
    }
}
