using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.Proxy
{
    public interface IBigDataProxy
    {
        void Write(IPartitionConfiguration configuration, JArray data);
        JArray Query(IPartitionConfiguration configuration, JArray parameters);
    }
}
