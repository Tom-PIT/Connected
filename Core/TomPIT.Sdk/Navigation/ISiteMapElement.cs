using TomPIT.Middleware;

namespace TomPIT.Navigation
{
	public interface ISiteMapElement : IMiddlewareObject
	{
		ISiteMapElement Parent { get; }
		string Text { get; }
		bool Visible { get; }
	}
}
