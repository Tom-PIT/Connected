using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class Behavior : EnvironmentClient, IDomElementBehavior
	{
		public Behavior(IEnvironment environment) : base(environment)
		{
		}

		public bool AutoExpand { get; set; } = true;
		public bool Static { get; set; } = true;
		public bool Container { get; set; } = false;
	}
}
