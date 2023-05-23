using System;
using System.Collections.Immutable;
using TomPIT.Diagnostics;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;

internal class LoggingManagementController : ILoggingManagementController
{
    public void Clean()
    {
        DataModel.Logging.Clear();
    }

    public void Delete(long id)
    {
        DataModel.Logging.Delete(id);
    }

    public ImmutableList<ILogEntry> Query(DateTime date, Guid component, Guid element, Guid metric)
    {
        return DataModel.Logging.Query(date, component, element).ToImmutableList();
    }
}
