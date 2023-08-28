using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Data.Storage;
using TomPIT.Reflection;

namespace TomPIT.Middleware.Storage;
internal sealed class ConnectionProvider : IConnectionProvider, IAsyncDisposable, IDisposable
{
	private List<IStorageConnection> _connections;

	public ConnectionProvider(IMiddlewareContext context, ITransactionContext transaction)
	{
		Context = context;
		TransactionService = transaction;
		TransactionService.StateChanged += OnTransactionStateChanged;
		_connections = new();
	}

	public IMiddlewareContext Context { get; }
	private ITransactionContext TransactionService { get; }
	private List<IStorageConnection> Connections => _connections;
	public StorageConnectionMode Mode { get; set; } = StorageConnectionMode.Shared;

	public async Task<ImmutableList<IStorageConnection>> Open<TEntity>(StorageContextArgs args)
	{
		/*
	  * Isolated transactions are supported only during active TransactionService state.
	  */
		if (TransactionService.State == MiddlewareTransactionState.Completed)
			Mode = StorageConnectionMode.Isolated;

		return args is ISchemaSynchronizationContext context ? await ResolveSingle(context) : await ResolveMultiple<TEntity>(args);
	}
	/// <summary>
	/// This method is called if the supplied arguments already provided connection type on which they will perform operations.
	/// </summary>
	/// <remarks>
	/// This method is usually called when synchronizing entities because the synchronization process already knows what connections
	/// should be used.
	/// </remarks>
	/// <param name="args"></param>
	/// <param name="behavior"></param>
	/// <returns></returns>
	/// <exception cref="NullReferenceException"></exception>
	private async Task<ImmutableList<IStorageConnection>> ResolveSingle(ISchemaSynchronizationContext args)
	{
		return new List<IStorageConnection> { await EnsureConnection(args.ConnectionType, args.ConnectionString) }.ToImmutableList();
	}

	private async Task<ImmutableList<IStorageConnection>> ResolveMultiple<TEntity>(StorageContextArgs args)
	{
		var connectionMiddleware = await ResolveConnectionMiddleware<TEntity>();
		/*
       * Check if sharding is supported on the entity
       */
		var shardingMiddleware = await ResolveShardingMiddleware<TEntity>();
		var result = new List<IStorageConnection>
		  {
			/*
			 * Default connection is always used regardless of sharding support
			 */
			await EnsureConnection(connectionMiddleware.ConnectionType, connectionMiddleware.DefaultConnectionString)
		  };

		if (shardingMiddleware is not null)
		{
			foreach (var node in await shardingMiddleware.ProvideNodes(args.Operation))
			{
				/*
			* Sharding is only supported on connection of the same type.
			*/
				if (!string.Equals(node.ConnectionType, connectionMiddleware.ConnectionType.FullName, StringComparison.Ordinal))
					throw new ArgumentException("Sharding connection types mismatch ({connectionType})", node.ConnectionType);

				if (Type.GetType(node.ConnectionType) is not Type connectionType)
					throw new NullReferenceException(node.ConnectionType);

				result.Add(await EnsureConnection(connectionType, node.ConnectionString));
			}
		}

		return result.ToImmutableList();
	}

	private async Task<IShardingMiddleware?> ResolveShardingMiddleware<TEntity>()
	{
		return await Tenant.GetService<IMiddlewareService>().First<IShardingMiddleware>(Context, CallerContext.Create(typeof(TEntity)));
	}

	private async Task<IStorageConnection> EnsureConnection(Type connectionType, string connectionString)
	{
		if (Mode == StorageConnectionMode.Shared
			 && Connections.FirstOrDefault(f => f.GetType() == connectionType
				  && string.Equals(f.ConnectionString, connectionString, StringComparison.OrdinalIgnoreCase)) is IStorageConnection existing)
		{
			return existing;
		}
		else
			return await CreateConnection(connectionType, connectionString, Mode);
	}

	private async Task<IStorageConnection> CreateConnection(Type connectionType, string connectionString, StorageConnectionMode behavior)
	{
		if (TypeExtensions.CreateInstance(connectionType) is not IStorageConnection newConnection)
			throw new NullReferenceException(connectionType.Name);

		await newConnection.Initialize(Context, this, new StorageConnectionArgs(connectionString, behavior));

		if (behavior == StorageConnectionMode.Shared)
			Connections.Add(newConnection);

		return newConnection;
	}

	private async Task<ISchemaMiddleware> ResolveConnectionMiddleware<TEntity>()
	{
		var items = await Tenant.GetService<IMiddlewareService>().Query<ISchemaMiddleware>(Context);

		if (items.IsEmpty)
			throw new NullReferenceException(nameof(ResolveConnectionMiddleware));

		foreach (var item in items)
		{
			if (await item.IsEntitySupported(typeof(TEntity)))
				return item;
		}

		throw new NullReferenceException(nameof(ResolveConnectionMiddleware));
	}
	private async void OnTransactionStateChanged(object? sender, EventArgs e)
	{
		if (TransactionService.State == MiddlewareTransactionState.Committing)
			await Commit();
		else if (TransactionService.State == MiddlewareTransactionState.Reverting)
			await Rollback();
	}

	private async Task Commit()
	{
		foreach (var connection in Connections)
			await connection.Commit();
	}

	private async Task Rollback()
	{
		foreach (var connection in Connections)
			await connection.Rollback();
	}
	public async ValueTask DisposeAsync()
	{
		if (_connections is not null)
		{
			foreach (var connection in _connections)
				await connection.DisposeAsync().ConfigureAwait(false);

			_connections = null;
		}

		Dispose(false);

		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_connections is not null)
			{
				foreach (var connection in _connections)
					connection.Dispose();

				_connections.Clear();
			}
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
