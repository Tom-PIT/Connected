using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Diagnostics;
using TomPIT.MicroServices.Design;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Distributed
{
	[DomDesigner(DesignUtils.ScheduleDesigner, Mode = EnvironmentMode.Runtime)]
	[DomDesigner(DomDesignerAttribute.TextDesigner, Mode = EnvironmentMode.Design)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class HostedWorker : SourceCodeConfiguration, IHostedWorkerConfiguration
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
