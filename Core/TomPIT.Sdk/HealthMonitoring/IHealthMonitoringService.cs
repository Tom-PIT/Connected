using Connected.SaaS.Clients.HealthMonitoring;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TomPIT.Runtime.Configuration;

namespace TomPIT.HealthMonitoring
{
	public interface IHealthMonitoringService
	{
		public bool TryLog(string message, TraceLevel level, MiddlewareHealthMonitoringConfiguration config);
	}
}
