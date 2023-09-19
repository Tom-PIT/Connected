using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Diagnostics;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management;
internal class MetricManagementController : IMetricManagementController
{
    private const string Controller = "MetricManagement";
    public void Clear(Guid component, Guid element)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Clear"), new
        {
            component,
            element
        });
    }

    public ImmutableList<IMetric> Query(DateTime date, Guid component, Guid element)
    {
        return Connection.Post<List<Metric>>(Connection.CreateUrl(Controller, "Query"), new
        {
            date,
            component,
            element
        }).ToImmutableList<IMetric>();
    }
}
