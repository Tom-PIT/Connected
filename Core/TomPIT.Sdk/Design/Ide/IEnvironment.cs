using Newtonsoft.Json.Linq;
using TomPIT.Design.Ide.Dom;
using TomPIT.Design.Ide.Globalization;
using TomPIT.Design.Ide.Selection;
using TomPIT.Middleware;
using TomPIT.Runtime;

namespace TomPIT.Design.Ide
{
	public interface IEnvironment
	{
		IDom Dom { get; }
		IGlobalizationProvider Globalization { get; }
		ISelectionProvider Selection { get; }
		IMicroServiceContext Context { get; }
		string Id { get; }
		JObject RequestBody { get; }
		string IdeUrl { get; }
		EnvironmentMode Mode { get; }
	}
}
