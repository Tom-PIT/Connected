using Newtonsoft.Json.Linq;
using TomPIT.Annotations;

namespace TomPIT.ActionResults
{
	public class SectionResult : Result, IDesignerActionResultSection
	{
		public SectionResult(object model, EnvironmentSection sections) : this(model, sections, null)
		{
		}

		public SectionResult(object model, EnvironmentSection sections, JObject data) : base(model)
		{
			Sections = sections;
			Data = data;
		}

		public EnvironmentSection Sections { get; set; }
		public JObject Data { get; }
	}
}
