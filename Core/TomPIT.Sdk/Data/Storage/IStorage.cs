using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Middleware.Interop;

namespace TomPIT.Data.Storage;

public enum DataConcurrencyMode
{
    Enabled = 1,
    Disabled
}

public interface IStorage<TEntity> : IQueryable<TEntity>, IEnumerable<TEntity>, IEnumerable, IQueryable, IOrderedQueryable<TEntity>, IOrderedQueryable where TEntity : IEntity
{
    Task<TEntity?> Update(TEntity? entity);
    Task<TEntity?> Update(TEntity? entity, IOperation operation, Func<Task<TEntity?>>? concurrencyRetrying);
    Task<TEntity?> Update(TEntity? entity, IOperation operation, Func<Task<TEntity?>>? concurrencyRetrying, Func<TEntity, Task<TEntity>>? merging);
    Task<ImmutableList<System.Data.IDataReader>> OpenReaders(StorageContextArgs args);
    Task<int> Execute(StorageContextArgs args);
    Task<IEnumerable<TEntity>> Query(StorageContextArgs args);
    Task<TEntity?> Select(StorageContextArgs args);
}
