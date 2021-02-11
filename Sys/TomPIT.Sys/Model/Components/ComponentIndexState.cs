using TomPIT.ComponentModel;

namespace TomPIT.Sys.Model.Components
{
	internal class ComponentIndexState : ComponentState, IComponentIndexState
	{
		public IndexState State { get; set; }
	}
}
