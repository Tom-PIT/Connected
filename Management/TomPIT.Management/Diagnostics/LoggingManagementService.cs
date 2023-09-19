using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Middleware;

namespace TomPIT.Management.Diagnostics
{
    internal class LoggingManagementService : TenantObject, ILoggingManagementService
    {
        public LoggingManagementService(ITenant tenant) : base(tenant)
        {

        }

        public void Clear()
        {
            Instance.SysProxy.Management.Logging.Clean();
        }

        public void Delete(long id)
        {
            var u = Tenant.CreateUrl("LoggingManagement", "Delete");

            var args = new JObject
            {
                "id", id
            };

            Tenant.Post(u, args);
        }

        public List<ILogEntry> Query(DateTime date)
        {
            return Instance.SysProxy.Management.Logging.Query(date, Guid.Empty, Guid.Empty, Guid.Empty).ToList();
        }

        public List<ILogEntry> Query(Guid metric)
        {
            return Instance.SysProxy.Management.Logging.Query(DateTime.Today, Guid.Empty, Guid.Empty, metric).ToList();
        }
    }
}
