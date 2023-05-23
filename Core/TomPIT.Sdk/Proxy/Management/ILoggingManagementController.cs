using System;
using System.Collections.Immutable;
using TomPIT.Diagnostics;

namespace TomPIT.Proxy.Management
{
    public interface ILoggingManagementController
    {
        void Clean();
        void Delete(long id);
        ImmutableList<ILogEntry> Query(DateTime date, Guid component, Guid element, Guid metric);
    }
}
