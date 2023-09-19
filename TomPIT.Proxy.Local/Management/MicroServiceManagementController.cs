using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management
{
    internal class MicroServiceManagementController : IMicroServiceManagementController
    {
        public void Delete(Guid token)
        {
            DataModel.MicroServices.Delete(token);
        }

        public void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, string meta, string version)
        {
            DataModel.MicroServices.Insert(token, name, status, resourceGroup, template, meta, version);
        }

        public ImmutableList<IMicroService> Query(Guid resourceGroup)
        {
            return DataModel.MicroServices.Query(resourceGroup).ToImmutableList();
        }

        public void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, UpdateStatus updateStatus, CommitStatus commitStatus, Guid package, Guid plan)
        {
            DataModel.MicroServices.Update(token, name, status, template, resourceGroup, package, plan, updateStatus, commitStatus);
        }
    }
}
