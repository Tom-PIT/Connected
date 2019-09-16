namespace TomPIT.Middleware
{
	public interface IAsyncOperation : IMiddlewareObject
	{
		bool Cancel { get; set; }

		IMiddlewareCallback Callback { get; }

		void SetAsyncState(bool async);
	}
}
