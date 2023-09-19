using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Diagnostics;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management;
internal class LoggingManagementController : ILoggingManagementController
{
    private const string Controller = "LoggingManagement";

    public void Clean()
    {
        Connection.Post(Connection.CreateUrl(Controller, "Clear"));
    }

    public void Delete(long id)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
        {
            id
        });
    }

    public ImmutableList<ILogEntry> Query(DateTime date, Guid component, Guid element, Guid metric)
    {
        return Connection.Post<List<LogEntry>>(Connection.CreateUrl(Controller, "Query"), new
        {
            date,
            component,
            element,
            metric
        }).ToImmutableList<ILogEntry>();
    }
}
