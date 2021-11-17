using Newtonsoft.Json.Linq;

namespace TomPIT.Models
{
	public interface IRuntimeModel : IModel, IActionContextProvider
	{
		JObject Arguments { get; }

		void MergeArguments(JObject arguments);

		void ReplaceArguments(JObject arguments);

		IRuntimeModel Clone();
	}
}
