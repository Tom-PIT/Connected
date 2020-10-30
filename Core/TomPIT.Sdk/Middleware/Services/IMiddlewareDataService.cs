namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareDataService
	{
		IMiddlewareDataAudit Audit { get; }
		IMiddlewareUserDataService User { get; }
		IMiddlewareLockingService Locking { get; }
	}
}
