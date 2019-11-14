namespace TomPIT.Middleware
{
	public interface IMiddlewareOperation : IMiddlewareComponent
	{
		IMiddlewareTransaction Begin();
	}
}
