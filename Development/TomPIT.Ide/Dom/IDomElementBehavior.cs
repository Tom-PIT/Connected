using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Dom
{
	public interface IDomElementBehavior : IEnvironmentObject
	{
		bool AutoExpand { get; }
		bool Static { get; }
		bool Container { get; }
	}
}
