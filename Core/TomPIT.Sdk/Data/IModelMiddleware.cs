using System.Collections.Generic;
using System.Threading.Tasks;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Data
{
    public interface IModelMiddleware<T> : IModelComponent
    {
        List<T> Query([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] params object[] e);

        T Select([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] params object[] e);

        Task<List<T>> QueryAsync([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] params object[] e);

        Task<T> SelectAsync([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] params object[] e);

        T CreateEntity(object instance);
        R CreateEntity<R>(object instance);

        T Merge(T entity, object instance);
    }
}
