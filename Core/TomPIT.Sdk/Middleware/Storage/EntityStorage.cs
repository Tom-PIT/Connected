using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Data;
using TomPIT.Data.DataProtection;
using TomPIT.Data.Storage;
using TomPIT.Middleware.Interop;
using TomPIT.Serialization;
using TomPIT.Validation;

namespace TomPIT.Middleware.Storage;
/// <summary>
/// Provides read and write operations on the supported storage providers.
/// </summary>
/// <typeparam name="TEntity">>The type of the entity on which operations are performed.</typeparam>
internal class EntityStorage<TEntity> : IAsyncEnumerable<TEntity>, IStorage<TEntity>
		where TEntity : IEntity
{
	private IQueryProvider? _provider;
	/// <summary>
	/// Creates a new <see cref="EntityStorage{TEntity}"/> instance.
	/// </summary>
	/// <param name="context">Middleware context containing services and saga orchestration.</param>
	/// <param name="connections">Middleware for providing storage connections.</param>
	/// <param name="transactions">Saga Transactions orchestration.</param>
	public EntityStorage(IMiddlewareContext context, IConnectionProvider connections,
		 ITransactionContext transactions)
	{
		Context = context;
		Connections = connections;
		Transactions = transactions;
		Expression = Expression.Constant(this);
	}
	/// <summary>
	/// The middleware used when performing entity operations.
	/// </summary>
	private IStorageMiddleware StorageMiddleware
	{
		get
		{
			if (Provider is not IStorageMiddleware result)
				throw new InvalidCastException(nameof(IStorageMiddleware));

			return result;
		}
	}
	/// <summary>
	/// The expression used for retrieving entities.
	/// </summary>
	public Expression Expression { get; }
	/// <summary>
	/// The entity type for which operations are performed.
	/// </summary>
	public Type ElementType => typeof(TEntity);
	/// <summary>
	/// The provider used when querying entities. It is based on the <see cref="ElementType"/>.
	/// </summary>
	public IQueryProvider Provider => _provider;
	/// <summary>
	/// Middleware used for validation in concurrency transactions.
	/// </summary>
	private IMiddlewareContext Context { get; }
	/// <summary>
	/// Middleware for retrieving storage connections.
	/// </summary>
	private IConnectionProvider Connections { get; }
	/// <summary>
	/// Middleware providing saga transactions orchestration. 
	/// </summary>
	private ITransactionContext Transactions { get; }
	/// <summary>
	/// Gets enumerator for entities retrieved via <see cref="Expression"/>.
	/// </summary>
	/// <returns>Enumerator containing entities.</returns>
	public IEnumerator<TEntity> GetEnumerator()
	{
		var result = Provider.Execute(Expression);
		/*
	  * Make sure we always return non nullable value.
	  */
		if (result is null)
			return new List<TEntity>().GetEnumerator();

		return ((IEnumerable<TEntity>)result).GetEnumerator();
	}
	/// <summary>
	/// Gets enumerator for entities retrieved via <see cref="Expression"/>.
	/// </summary>
	/// <returns>Enumerator containing entities.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		var result = Provider.Execute(Expression);
		/*
	  * Make sure we always return non nullable value.
	  */
		if (result is null)
			return new List<TEntity>().GetEnumerator();

		return ((IEnumerable)result).GetEnumerator();
	}
	/// <summary>
	/// Gets enumerator for asynchronous entity retrieval via <see cref="Expression"/>.
	/// </summary>
	/// <param name="cancellationToken">Token that enables operation to be cancelled.</param>
	/// <returns>Asynchronous enumerator containing entities.</returns>
	public async IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		var result = Provider.Execute(Expression);

		if (result is IEnumerable en)
		{
			var enumerator = en.GetEnumerator();

			while (enumerator.MoveNext())
			{
				await Task.CompletedTask;

				yield return (TEntity)enumerator.Current;
			}
		}
	}

	public override string ToString()
	{
		if (Expression.NodeType == ExpressionType.Constant && ((ConstantExpression)Expression).Value == this)
			return "Query(" + typeof(TEntity) + ")";
		else
			return Expression.ToString();
	}
	/// <summary>
	/// Performs the update on the specified entity.
	/// </summary>
	/// <param name="entity">The entity to be updated.</param>
	/// <returns>Entity with an id if insert has been executed, the same entity otherwise.</returns>
	/// <exception cref="NullReferenceException">Thrown if the storage statement could no be created.</exception>
	public async Task<TEntity?> Update(TEntity? entity)
	{
		if (entity is null)
			return entity;

		await InvokeEntityProtection(entity);

		var operation = StorageMiddleware.CreateOperation(entity);

		await Execute(new StorageContextArgs(operation));

		var clone = entity.Clone();

		ReturnValueBinder.Bind(operation, clone);

		return clone;
	}
	/// <summary>
	/// Performs the update on the specified entity with optional concurrency callback support.
	/// </summary>
	/// <param name="entity">The entity to update.</param>
	/// <param name="operation">The operation that supplies updated values.</param>
	/// <param name="concurrencyRetrying">The retry delegate for preparing new update.</param>
	/// <returns>The entity with the newly inserted id if insert was performed, the same entity otherwise.</returns>
	public async Task<TEntity?> Update(TEntity? entity, IOperation operation, Func<Task<TEntity?>>? concurrencyRetrying)
	{
		return await Update(entity, operation, concurrencyRetrying, null);
	}
	/// <summary>
	/// Updates the entity to the underlying storage with concurrency check.
	/// </summary>
	/// <param name="entity">The entity to update.</param>
	/// <param name="operation">The operation that supplied updated values.</param>
	/// <param name="concurrencyRetrying">The retry delegate for preparing new update.</param>
	/// <param name="merging">An optional merge callback if default merge is not sufficient.</param>
	/// <returns>The entity with the newly inserted id if insert was performed, the same entity otherwise.</returns>
	public async Task<TEntity?> Update(TEntity? entity, IOperation operation, Func<Task<TEntity?>>? concurrencyRetrying, Func<TEntity, Task<TEntity>>? merging)
	{
		if (entity is null)
			return entity;

		DBConcurrencyException? lastException = null;
		/*
		 * Merge the updating entity with the supplied arguments. If callback is provided it is used instead of the default merge.
		 */
		var currentEntity = merging is null ? entity.Merge(operation, entity.Verb) : await merging(entity);
		/*
		 * There will be 3 retries. If none succeeds exception will be thrown.
		 */
		for (var i = 0; i < 3; i++)
		{
			try
			{
				/*
				 * Perform the update. Note that provider should check for concurrency only if 
				 * the entity is updating. Concurrency is not used for inserting and deleting operations.
				 */
				await Update(currentEntity);
				/*
				 * Provider will merge the updating entity with a new id if the operation is Insert. For updating and
				 * deleting operations the same entity is returned.
				 */
				return currentEntity;
			}
			catch (DBConcurrencyException ex)
			{
				/*
				 * Concurrency exception occurred. If the callback is not passed we return immediately.
				 */
				if (concurrencyRetrying is null)
					throw;

				lastException = ex;
				/*
				 * Wait a small amount of time if the system is currently under heavy load to increase the probability
				 * of successful update.
				 */
				await Task.Delay(i * i * 50);
				/*
				 * We must perform validation again since the state of the entities has possibly changed.
				 */
				if (await Tenant.GetService<IMiddlewareService>().Query<IValidator>(Context, CallerContext.Create(operation)) is ImmutableList<IValidator> items)
				{
					foreach (var item in items)
						await item.Validate();
				}
				await operation.ValidateAsync();
				/*
				 * If validation succeeded invoke callback which usually refreshes the cache which causes the entity to be
				 * reloaded from the data source.
				 */
				currentEntity = await concurrencyRetrying();
				/*
				 * Entity must be supplied and the merge is performed again.
				 */
				if (currentEntity is not null)
					currentEntity = merging is null ? currentEntity.Merge(operation, entity.Verb) : await merging(currentEntity);
				else
					throw new NullReferenceException(nameof(entity));
			}
		}
		/*
		 * This is not good. We couldn't update the entity after 3 retries. The system is most probably either under heavy load and
		 * the entity is updating very frequently.
		 */
		if (lastException is not null)
			throw lastException;

		return default;
	}
	/// <summary>
	/// Executes storage operation against one or mode storages.
	/// </summary>
	/// <param name="args">The arguments containing data about operation to be performed.</param>
	/// <returns>The number of records affected in the physical storage.</returns>
	/// <exception cref="DBConcurrencyException">If concurrency is supported on the operation and 
	/// no records have been affected and the actual operation is UPDATE this exception is thrown.</exception>
	public async Task<int> Execute(StorageContextArgs args)
	{
		await using var writer = await OpenWriter(args);
		/*
	  * Execute operation against the storage. This method should return the number of records affected.
	  */
		var recordsAffected = await writer.Execute();
		/*
	  * It is not necessary that concurrency is actually considered. Concurrency should be disabled
	  * if the operation is not UPDATE or the entity does not supports it (does not have an Etag or similar property).
	  */
		if (recordsAffected == 0 && args.Operation.Concurrency == DataConcurrencyMode.Enabled)
			throw new DBConcurrencyException($"{SR.ErrConcurrencyMessage} ({typeof(IEntity).Name})");

		return recordsAffected;
	}
	/// <summary>
	/// Opens one or more readers for the specified entity.
	/// </summary>
	/// <remarks>
	/// If the entity does not support sharding, only one reader is returned. If arguments require more
	/// than one shard to be read this method will return one <see cref="IStorageReader{T}"/> for every shard.
	/// </remarks>
	/// <param name="args">The arguments containing data about operation to be performed.</param>
	/// <returns>One or more <see cref="IStorageReader{T}"/>.</returns>
	private async Task<ImmutableList<IStorageReader<TEntity>>> OpenEntityReaders(StorageContextArgs args)
	{
		/*
	  * Connection middleware will return one connection for every shard. If sharding is not supported only
	  * one connection will be returned.
	  */
		var connections = await Connections.Open<TEntity>(args);
		var result = new List<IStorageReader<TEntity>>();

		foreach (var connection in connections)
			result.Add(OpenReader(args.Operation, connection));

		return result.ToImmutableList();
	}
	/// <summary>
	/// Opens one or more readers for the specified entity.
	/// </summary>
	/// <param name="args">The arguments containing data about operation to be performed.</param>
	/// <returns>One or more <see cref="IDataReader{T}"/>.</returns>
	public async Task<ImmutableList<IDataReader>> OpenReaders(StorageContextArgs args)
	{
		/*
	  * Connection middleware will return one connection for every shard. If sharding is not supported only
	  * one connection will be returned.
	  */
		var connections = await Connections.Open<TEntity>(args);
		var result = new List<IDataReader>();

		foreach (var connection in connections)
		{
			/*
		 * Temporarily create a full database reader. We won't actually need it but it is
		 * the only way to get to the actual IDataReader.
		 */
			await using var r = StorageMiddleware.OpenReader<TEntity>(args.Operation, connection);
			/*
		 * Now open reader and add it to the result.
		 */
			result.Add(await r.OpenReader());
		}

		return result.ToImmutableList();
	}
	/// <summary>
	/// Opens the <see cref="IStorageReader{T}"/> on the underlying connection.
	/// </summary>
	/// <param name="operation">The operation to be performed on the data reader.</param>
	/// <param name="connection">The connection to be used when opening the reader.</param>
	/// <returns>The <see cref="IStorageReader{T}"/>.</returns>
	private IStorageReader<TEntity> OpenReader(IStorageOperation operation, IStorageConnection connection)
	{
		return StorageMiddleware.OpenReader<TEntity>(operation, connection);
	}
	/// <summary>
	/// Opens the <see cref="IStorageWriter"/>  on the underlying connection.
	/// </summary>
	/// <param name="args">The arguments containing operation to be performed.</param>
	/// <returns>The <see cref="IStorageWriter"/>.</returns>
	private async Task<IStorageWriter> OpenWriter(StorageContextArgs args)
	{
		var connections = await Connections.Open<TEntity>(args);
		/*
	  * Only one connection should be returned when performing transactions
	  * on the single entity.
	  */
		if (connections.Count != 1)
			throw new InvalidOperationException("Only one connection expected.");

		return OpenWriter(args.Operation, connections[0]);
	}
	/// <summary>
	/// Opens the <see cref="IStorageWriter"/>  on the underlying connection.
	/// </summary>
	/// <param name="operation">The operation to be performed.</param>
	/// <param name="connection">The connection to be used on the writer.</param>
	/// <returns>The <see cref="IStorageWriter"/>.</returns>
	private IStorageWriter OpenWriter(IStorageOperation operation, IStorageConnection connection)
	{
		/*
	  * Signal transaction orchestration that we are going to use transactions.
	  */
		Transactions.IsDirty = true;

		return StorageMiddleware.OpenWriter(operation, connection);
	}
	/// <summary>
	/// Performs a query for the specified operation.
	/// </summary>
	/// <param name="args">Arguments containing data about operation to be performed.</param>
	/// <returns>A List of entities that were returned from the storage.</returns>
	public async Task<IEnumerable<TEntity>> Query(StorageContextArgs args)
	{
		var readers = await OpenEntityReaders(args);
		/*
	  * In a sharding model it is possible that more than one reader will be returned since
	  * data could reside in more than one shard, for example:
	  * we have projects, each having its work items in its own shard. It's fine to query
	  * work items for the project since they are definitely in the same shard. But what about 
	  * querying work items for the specific user. If the user has access to the more then one
	  * project it is very likely that work items are in more than one shard.
	  */
		if (readers.Count == 1)
		{
			var result = await readers[0].Query();

			await readers[0].DisposeAsync();

			return result;
		}
		else
		{
			/*
		 * It's a sharding scenario
		 */
			var results = new List<TEntity>();
			var tasks = new List<Task>();

			foreach (var reader in readers)
			{
				tasks.Add(Task.Run(async () =>
				{
					if (await reader.Query() is ImmutableList<TEntity> r && !r.IsEmpty)
					{
						lock (results)
							results.AddRange(r);
					}
				}));
			}

			await Task.WhenAll(tasks);

			/*
		 * Need to manually dispose all readers.
		 */
			foreach (var reader in readers)
				await reader.DisposeAsync();

			return results.ToImmutableList();
		}
	}
	/// <summary>
	/// Performs a single entity select for the specified operation.
	/// </summary>
	/// <param name="args">Arguments containing data about operation to be performed.</param>
	/// <returns>An entity if found, <c>null</c> otherwise.</returns>
	public async Task<TEntity?> Select(StorageContextArgs args)
	{
		var readers = await OpenEntityReaders(args);
		/*
	  * In a sharding model, it is possible that a middleware won't know
	  * exactly in which shard the record resides. This is not ideal but very much
	  * possible scenario. This is why we will perform a call on all available
	  * readers and then, if more then one record returned, selects only the first one.
	  * -------------------------------------------------------------------------
	  * Q: should we throw an exception if more than one record is found?
	  * -------------------------------------------------------------------------
	  */
		TEntity? result = default;

		if (readers.Count == 1)
			result = await readers[0].Select();
		else
		{
			var results = new List<TEntity>();
			var tasks = new List<Task>();

			foreach (var reader in readers)
			{
				tasks.Add(Task.Run(async () =>
				{
					if (await reader.Select() is TEntity r)
					{
						lock (results)
							results.Add(r);
					}
				}));
			}

			await Task.WhenAll(tasks);

			if (results.Any())
				result = results[0];
		}
		/*
	  * Need to manually dispose all readers.
	  */
		foreach (var reader in readers)
			await reader.DisposeAsync();

		return result;
	}
	/// <summary>
	/// Resolved provider used based on the entity type.
	/// </summary>
	/// <returns></returns>
	/// <exception cref="NullReferenceException"></exception>
	private async Task ResolveProvider()
	{
		/*
  	    * We need to resolve provider based on an entity type. At least one
	    * provider must respond to the entity type. On the other hand, only
	    * one provider should handle entity type. This means sharding is not
	    * supported on nodes with different connection types.
	    */
		var items = await Tenant.GetService<IMiddlewareService>().Query<IStorageMiddleware>(Context);

		if (items.IsEmpty)
			throw new NullReferenceException($"No middleware for {nameof(IStorageMiddleware)} -> {ElementType.Name}");

		foreach (var item in items)
		{
			if (item.SupportsEntity(typeof(TEntity)))
			{
				_provider = item;
				break;
			}
		}

		if (_provider is null)
			throw new NullReferenceException($"{nameof(IStorageMiddleware)} -> {ElementType.Name}");
	}

	private async Task InvokeEntityProtection<TEntity>(TEntity entity)
	{
		var items = await Tenant.GetService<IMiddlewareService>().Query<IEntityProtectionMiddleware>(Context);

		if (items.IsEmpty)
			return;

		foreach (var item in items)
		{
			Serializer.Populate(entity, item);
			await item.Invoke();
		}
	}

	public async Task Initialize()
	{
		await ResolveProvider();
	}
}
