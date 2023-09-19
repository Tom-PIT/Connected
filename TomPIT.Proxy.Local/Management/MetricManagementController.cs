using System;
using System.Collections.Immutable;
using TomPIT.Diagnostics;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class MetricManagementController : IMetricManagementController
{
    public void Clear(Guid component, Guid element)
    {
        DataModel.Metrics.Clear(component, element);
    }

    public ImmutableList<IMetric> Query(DateTime date, Guid component, Guid element)
    {
        return DataModel.Metrics.Query(date, component, element).ToImmutableList();
    }
}
