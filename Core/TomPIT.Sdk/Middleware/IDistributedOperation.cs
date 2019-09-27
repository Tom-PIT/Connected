namespace TomPIT.Middleware
{
	public interface IDistributedOperation : IMiddlewareObject
	{
		bool Cancel { get; set; }

		IMiddlewareCallback Callback { get; }
	}
}
