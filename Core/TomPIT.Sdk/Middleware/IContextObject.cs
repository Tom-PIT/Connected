using Newtonsoft.Json;

namespace TomPIT.Middleware
{
	public interface IContextObject<T> where T : IMiddlewareContext
	{
		[JsonIgnore]
		T Context { get; }
	}
}
