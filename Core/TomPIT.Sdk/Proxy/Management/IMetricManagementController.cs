using System;
using System.Collections.Immutable;
using TomPIT.Diagnostics;

namespace TomPIT.Proxy.Management
{
    public interface IMetricManagementController
    {
        void Clear(Guid component, Guid element);
        ImmutableList<IMetric> Query(DateTime date, Guid component, Guid element);
    }
}
