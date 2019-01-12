using Newtonsoft.Json.Linq;
using TomPIT.Annotations;

namespace TomPIT.Design
{
	public interface ITransactionResult
	{
		bool Success { get; }
		EnvironmentSection Invalidate { get; }
		object Component { get; }
		JObject Data { get; }
	}
}
