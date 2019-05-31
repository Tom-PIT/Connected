using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.Workers;

namespace TomPIT.Application.Workers
{
	[DefaultEvent(nameof(Invoke))]
	public class QueueWorker : ComponentConfiguration, IQueueWorker
	{
		public const string ComponentCategory = "Queue";

		private IServerEvent _invoke = null;
		private IMetricConfiguration _metric = null;

		[EventArguments(typeof(QueueInvokeArgs))]
		public IServerEvent Invoke
		{
			get
			{
				if (_invoke == null)
					_invoke = new ServerEvent { Parent = this };

				return _invoke;
			}
		}

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
	}
}
