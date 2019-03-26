using Newtonsoft.Json.Linq;
using TomPIT.Services;

namespace TomPIT.Models
{
	public interface IRuntimeViewModel : IExecutionContext
	{
		JObject Arguments { get; }
	}
}
