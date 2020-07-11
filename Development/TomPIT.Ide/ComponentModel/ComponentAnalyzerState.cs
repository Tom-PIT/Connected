using TomPIT.ComponentModel;
using TomPIT.Design;

namespace TomPIT.Ide.ComponentModel
{
	public class ComponentAnalyzerState : ComponentState, IComponentAnalyzerState
	{
		public AnalyzerState State { get; set; }
	}
}
