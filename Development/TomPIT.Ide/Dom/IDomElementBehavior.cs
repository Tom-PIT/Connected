using TomPIT.Ide;

namespace TomPIT.Dom
{
	public interface IDomElementBehavior : IEnvironmentClient
	{
		bool AutoExpand { get; }
		bool Static { get; }
		bool Container { get; }
	}
}
