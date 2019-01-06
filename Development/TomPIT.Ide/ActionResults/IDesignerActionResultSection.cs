using TomPIT.Annotations;
using TomPIT.Ide;

namespace TomPIT.ActionResults
{
	public interface IDesignerActionResultSection : IDesignerActionResult
	{
		EnvironmentSection Sections { get; }
	}
}