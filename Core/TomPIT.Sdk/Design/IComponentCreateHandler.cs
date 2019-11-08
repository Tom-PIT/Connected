using TomPIT.Middleware;

namespace TomPIT.Design
{
	public interface IComponentCreateHandler
	{
		void InitializeNewComponent(IMiddlewareContext context, object instance);
	}
}
