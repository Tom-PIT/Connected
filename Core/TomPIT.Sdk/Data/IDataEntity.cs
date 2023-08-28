using Newtonsoft.Json.Linq;
using System;
using System.Data;
using TomPIT.Middleware;

namespace TomPIT.Data
{
    public interface IDataEntity
    {
        string Serialize();
        void Deserialize(JObject state);
        [Obsolete("This method is not used. Please use DataSource(IDataReader) instead.")]
        void DataSource(JObject state);
        void Deserialize(IMiddlewareContext context, IDataReader reader);

        T Evolve<T>() where T : class, IDataEntity;
    }
}
