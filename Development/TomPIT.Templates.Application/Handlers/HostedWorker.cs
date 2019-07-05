using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.Workers;

namespace TomPIT.Application.Workers
{
	[DomDesigner("TomPIT.Designers.ScheduleDesigner, TomPIT.Management", Mode = Services.EnvironmentMode.Runtime)]
	[DomDesigner(DomDesignerAttribute.TextDesigner, Mode = Services.EnvironmentMode.Design)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class HostedWorker : ComponentConfiguration, IHostedWorker
	{
		public const string ComponentCategory = "Worker";

		private IMetricConfiguration _metric = null;

		[EnvironmentVisibility(Services.EnvironmentMode.Runtime)]
		public IMetricConfiguration Metrics
		{
			get
			{
				if (_metric == null)
					_metric = new MetricConfiguration { Parent = this };

				return _metric;
			}
		}

		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
