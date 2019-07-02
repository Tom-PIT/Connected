using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.Handlers;
using TomPIT.ComponentModel.Workers;

namespace TomPIT.Application.Workers
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class QueueHandlerConfiguration : ComponentConfiguration, IQueueHandlerConfiguration
	{
		public const string ComponentCategory = "Queue";

		private IServerEvent _invoke = null;
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
