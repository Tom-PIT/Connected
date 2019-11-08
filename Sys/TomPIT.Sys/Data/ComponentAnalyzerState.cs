using TomPIT.ComponentModel;

namespace TomPIT.Sys.Data
{
	internal class ComponentAnalyzerState : ComponentState, IComponentAnalyzerState
	{
		public AnalyzerState State { get; set; }
	}
}
