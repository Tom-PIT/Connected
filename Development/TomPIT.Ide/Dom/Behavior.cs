using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Dom
{
	public class Behavior : EnvironmentObject, IDomElementBehavior
	{
		public Behavior(IEnvironment environment) : base(environment)
		{
		}

		public bool AutoExpand { get; set; } = true;
		public bool Static { get; set; } = true;
		public bool Container { get; set; } = false;
	}
}
