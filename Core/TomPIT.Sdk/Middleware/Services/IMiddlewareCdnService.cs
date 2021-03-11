namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareCdnService
	{
		IMiddlewareEmail Mail { get; }
		IMiddlewareSubscriptions Subscriptions { get; }
		IMiddlewareEvents Events { get; }
		IMiddlewarePrinting Printing { get; }
		IMiddlewareQueue Queue { get; }
		IMiddlewareClient Clients { get; }
		IMiddlewareDocuments Documents { get; }
	}
}
