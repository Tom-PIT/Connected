using TomPIT.Data;

namespace TomPIT.Middleware
{
	public interface IMiddlewareConnectionBag
	{
		void Push(IDataConnection connection);
	}
}
