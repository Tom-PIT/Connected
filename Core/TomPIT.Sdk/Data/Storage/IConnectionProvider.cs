using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace TomPIT.Data.Storage;
/// <summary>
/// This middleware provides one or more connection for the specified arguments.
/// </summary>
/// <remarks>
/// If entity supports sharding (provided by <see cref="IShardingMiddleware"/>) it is possible that
/// more than one connection will be returned. For the transactions only one connection is tipicaly returned
/// since only one entity at a time is usually performed. For query operations the scenario could be more complex 
/// because data could reside in more than one shard. In that case one connection for each shard will be returned.
/// </remarks>
public interface IConnectionProvider : IDisposable
{
    /// <summary>
    /// Opens one or more connections for the specified arguments. 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity on which storage operations will be performed.</typeparam>
    /// <param name="args">The arguments describing what operation is about to be performed.</param>
    /// <returns>One or more storage connections. One connection if entity does not supports sharding, more if
    /// it supports it and the operation requires data from more than one shard.</returns>
    Task<ImmutableList<IStorageConnection>> Open<TEntity>(StorageContextArgs args);

    StorageConnectionMode Mode { get; set; }
}
