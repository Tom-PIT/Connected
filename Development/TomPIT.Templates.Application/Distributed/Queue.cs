using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Diagnostics;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Distributed
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class Queue : SourceCodeConfiguration, IQueueConfiguration
	{
		private IMetricOptions _metric = null;

		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public IMetricOptions Metrics
		{
			get
			{
				if (_metric == null)
					_metric = new MetricOptions { Parent = this };

				return _metric;
			}
		}
	}
}
