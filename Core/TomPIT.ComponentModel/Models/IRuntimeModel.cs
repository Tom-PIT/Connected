using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.UI;
using TomPIT.Services;

namespace TomPIT.Models
{
	public interface IRuntimeModel : IExecutionContext, IRequestContextProvider
	{
		JObject Arguments { get; }

		void MergeArguments(JObject arguments);
	}
}
