using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;

namespace TomPIT.Design.Ide.Designers
{
	public interface IDesignerActionResultSection : IDesignerActionResult
	{
		EnvironmentSection Sections { get; }
		JObject Data { get; }
	}
}