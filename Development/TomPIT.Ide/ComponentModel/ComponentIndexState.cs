using TomPIT.ComponentModel;

namespace TomPIT.Ide.ComponentModel
{
	public class ComponentIndexState : ComponentState, IComponentIndexState
	{
		public IndexState State { get; set; }
	}
}
