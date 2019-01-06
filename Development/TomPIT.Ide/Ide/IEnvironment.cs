using Newtonsoft.Json.Linq;
using TomPIT.Dom;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Ide
{
	public interface IEnvironment
	{
		IDom Dom { get; }
		IGlobalization Globalization { get; }
		ISelection Selection { get; }
		IExecutionContext Context { get; }

		string Id { get; }

		JObject RequestBody { get; }

		string IdeUrl { get; }
	}
}
