using System;
using Newtonsoft.Json;

namespace TomPIT.Middleware
{
	public interface IContextObject<T> : IDisposable where T : IMiddlewareContext
	{
		[JsonIgnore]
		T Context { get; }
	}
}
