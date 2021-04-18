using TomPIT.Middleware;

namespace TomPIT.Configuration
{
	public interface ISetting<T> : IMiddlewareComponent
	{
		T Value { get; set; }
	}
}
