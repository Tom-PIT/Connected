﻿using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Diagnostics;
using TomPIT.MicroServices.Design;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Distributed
{
	public class Queue : ComponentConfiguration, IQueueConfiguration
	{
		private ListItems<IQueueWorker> _ops = null;
		private IMetricOptions _metric = null;

		[Items(DesignUtils.QueueWorkerItems)]
		public ListItems<IQueueWorker> Workers
		{
			get
			{
				if (_ops == null)
					_ops = new ListItems<IQueueWorker> { Parent = this };

				return _ops;
			}
		}

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