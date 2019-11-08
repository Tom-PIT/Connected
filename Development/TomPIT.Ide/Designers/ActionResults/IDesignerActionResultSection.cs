using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;

namespace TomPIT.Ide.Designers.ActionResults
{
	public interface IDesignerActionResultSection : IDesignerActionResult
	{
		EnvironmentSection Sections { get; }
		JObject Data { get; }
	}
}