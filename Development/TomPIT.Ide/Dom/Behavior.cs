using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class Behavior : EnvironmentClient, IDomElementBehavior
	{
		public Behavior(IEnvironment environment) : base(environment)
		{
		}

		public bool AutoExpand { get; set; } = true;
	}
}
