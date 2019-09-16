using Newtonsoft.Json.Linq;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment.Providers;
using TomPIT.Middleware;

namespace TomPIT.Ide.Environment
{
	public interface IEnvironment
	{
		IDom Dom { get; }
		IGlobalizationProvider Globalization { get; }
		ISelectionProvider Selection { get; }
		IMiddlewareContext Context { get; }
		string Id { get; }
		JObject RequestBody { get; }
		string IdeUrl { get; }
	}
}
