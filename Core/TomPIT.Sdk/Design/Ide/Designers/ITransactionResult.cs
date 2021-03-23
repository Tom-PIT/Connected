using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;

namespace TomPIT.Design.Ide.Designers
{
	public interface ITransactionResult
	{
		bool Success { get; }
		EnvironmentSection Invalidate { get; }
		object Component { get; }
		JObject Data { get; }
	}
}
