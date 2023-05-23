using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Proxy.Management
{
    public interface IMicroServiceManagementController
    {
        void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, string meta, string version);
        void Delete(Guid token);
        void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, UpdateStatus updateStatus, CommitStatus commitStatus, Guid package, Guid plan);
        ImmutableList<IMicroService> Query(Guid resourceGroup);

    }
}
