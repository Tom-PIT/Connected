﻿using TomPIT.ComponentModel.Diagnostics;

namespace TomPIT.ComponentModel.Distributed
{
	public interface IWorkerConfiguration : IConfiguration, INamespaceElement
	{
		IMetricOptions Metrics { get; }
	}
}
