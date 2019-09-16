using TomPIT.Middleware;

namespace TomPIT.Data
{
	public abstract class DataModel : MiddlewareObject
	{
		protected DataModel(IMiddlewareContext context) : base(context)
		{

		}
	}
}
