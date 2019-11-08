using TomPIT.ComponentModel;

namespace TomPIT.Sys.Data
{
	internal class ComponentIndexState : ComponentState, IComponentIndexState
	{
		public IndexState State { get; set; }
	}
}
