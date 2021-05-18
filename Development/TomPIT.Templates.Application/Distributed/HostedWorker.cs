using System.ComponentModel;
using TomPIT.Annotations;
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
	public class HostedWorker : TextConfiguration, IHostedWorkerConfiguration
	{
		private IMetricOptions _metric = null;

		[Browsable(false)]
		public IMetricOptions Metrics
		{
			get
			{
				if (_metric == null)
					_metric = new MetricOptions { Parent = this };

				return _metric;
			}
		}

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Namespace { get; set; }
		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";

		public int DisableTreshold { get; set; } = 3;

		public int RetryInterval { get; set; } = 10;
	}
}
