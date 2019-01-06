using TomPIT.Annotations;
using TomPIT.Ide;

namespace TomPIT.ActionResults
{
	public class SectionResult : Result, IDesignerActionResultSection
	{
		public SectionResult(object model, EnvironmentSection sections) : base(model)
		{
			Sections = sections;
		}

		public EnvironmentSection Sections { get; private set; }
	}
}
