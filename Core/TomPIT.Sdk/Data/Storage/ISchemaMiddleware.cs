using System;
using System.Threading.Tasks;
using TomPIT.Data.Schema;
using TomPIT.Middleware;

namespace TomPIT.Data.Storage;
public interface ISchemaMiddleware : IMiddleware
{
	Task<bool> IsEntitySupported(Type entity);
	Task Synchronize(Type entity, ISchema schema);
	Type ConnectionType { get; }
	string DefaultConnectionString { get; }
}
