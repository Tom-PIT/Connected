using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;

namespace TomPIT.Management.Diagnostics
{
    internal class MetricManagementService : TenantObject, IMetricManagementService
    {
        public MetricManagementService(ITenant tenant) : base(tenant)
        {

        }

        public void Clear()
        {
            Clear(Guid.Empty);
        }

        public void Clear(Guid component)
        {
            Clear(component, Guid.Empty);
        }

        public void Clear(Guid component, Guid element)
        {
            Instance.SysProxy.Management.Metrics.Clear(component, element);
        }

        public List<IMetric> Query(DateTime date, Guid component)
        {
            return Query(date, component, Guid.Empty);
        }

        public List<IMetric> Query(DateTime date, Guid component, Guid element)
        {
            return Instance.SysProxy.Management.Metrics.Query(date, component, element).ToList();
        }
    }
}
