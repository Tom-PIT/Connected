using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Data.Storage;
public interface IShardingMiddleware : IMiddleware
{
	Task<ImmutableArray<IShardingNode>> ProvideNodes(IStorageOperation operation);
	Task<ImmutableArray<IShardingNode>> ProvideNodes(Type entityType);
}
