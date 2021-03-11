using TomPIT.ComponentModel;

namespace TomPIT.Sys.Model.Components
{
	internal class ComponentAnalyzerState : ComponentState, IComponentAnalyzerState
	{
		public AnalyzerState State { get; set; }
	}
}
