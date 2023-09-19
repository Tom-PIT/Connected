using System.Collections.Generic;

namespace TomPIT.Data.Storage;
public interface IStorageExecutor
{
    IEnumerable<TResult?> Execute<TResult>(IStorageOperation operation)
        where TResult : IEntity;
}
