using System;
using System.Linq;
using TomPIT.Middleware;

namespace TomPIT.Data.Storage;
public interface IStorageMiddleware : IQueryProvider, IMiddleware
{
	bool SupportsEntity(Type entityType);
	IStorageOperation CreateOperation<TEntity>(TEntity entity)
		 where TEntity : IEntity;

	IStorageReader<TEntity> OpenReader<TEntity>(IStorageOperation operation, IStorageConnection connection);
	IStorageWriter OpenWriter(IStorageOperation operation, IStorageConnection connection);
}
