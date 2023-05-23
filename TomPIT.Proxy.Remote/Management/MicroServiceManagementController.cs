using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management
{
    internal class MicroServiceManagementController : IMicroServiceManagementController
    {
        private const string Controller = "MicroServiceManagement";
        public void Delete(Guid token)
        {
            Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
            {
                microService = token
            });
        }

        public void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, string meta, string version)
        {
            Connection.Post(Connection.CreateUrl(Controller, "Insert"), new
            {
                microService = token,
                name,
                resourceGroup,
                template,
                status,
                meta,
                version
            });

        }

        public ImmutableList<IMicroService> Query(Guid resourceGroup)
        {
            return Connection.Get<List<MicroService>>(Connection.CreateUrl(Controller, "Query").AddParameter("resourceGroup", resourceGroup)).ToImmutableList<IMicroService>();
        }

        public void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, UpdateStatus updateStatus, CommitStatus commitStatus, Guid package, Guid plan)
        {
            Connection.Post(Connection.CreateUrl(Controller, "Update"), new
            {
                microService = token,
                name,
                resourceGroup,
                template,
                status,
                updateStatus,
                commitStatus,
                package,
                plan
            });
        }
    }
}
