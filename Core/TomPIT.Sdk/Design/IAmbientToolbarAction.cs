using TomPIT.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Design
{
	public interface IAmbientToolbarAction
	{
		string Glyph { get; }
		string Text { get; }
		string Action { get; }

		void Invoke(IMiddlewareContext context, IElement element);
	}
}
