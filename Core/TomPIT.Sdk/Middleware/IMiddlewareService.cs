using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace TomPIT.Middleware;
public interface IMiddlewareService
{
	Task<TMiddleware?> First<TMiddleware>(IMiddlewareContext context) where TMiddleware : IMiddleware;
	Task<TMiddleware?> First<TMiddleware>(IMiddlewareContext context, ICallerContext? caller) where TMiddleware : IMiddleware;

	Task<ImmutableList<TMiddleware>> Query<TMiddleware>(IMiddlewareContext context) where TMiddleware : IMiddleware;

	Task<ImmutableList<TMiddleware>> Query<TMiddleware>(IMiddlewareContext context, ICallerContext? caller) where TMiddleware : IMiddleware;

	Task<IMiddleware?> First(IMiddlewareContext context, Type type);
	Task<IMiddleware?> First(IMiddlewareContext context, Type type, ICallerContext? caller);

	Task<ImmutableList<IMiddleware>> Query(IMiddlewareContext context, Type type);

	Task<ImmutableList<IMiddleware>> Query(IMiddlewareContext context, Type type, ICallerContext? caller);
}
