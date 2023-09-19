using System;
using System.Collections.Generic;
using TomPIT.Environment;

namespace TomPIT.Management.Environment
{
    public interface IResourceGroupManagementService
    {
        Guid Insert(string name, Guid storageProvider, string connectionString);
        void Update(Guid token, string name, Guid storageProvider, string connectionString);
        void Delete(Guid token);

        List<IServerResourceGroup> Query();
    }
}
