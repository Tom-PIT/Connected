using TomPIT.Ide;

namespace TomPIT.Dom
{
	public interface IDomElementBehavior : IEnvironmentClient
	{
		bool AutoExpand { get; }
	}
}
