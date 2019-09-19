using TomPIT.ComponentModel;

namespace TomPIT.Ide.ComponentModel
{
	public class ComponentAnalyzerState : ComponentState, IComponentAnalyzerState
	{
		public AnalyzerState State { get; set; }
	}
}
