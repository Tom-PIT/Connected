using System;
using System.Collections.Generic;
using TomPIT.Environment;

namespace TomPIT.SysDb.Environment
{
    public interface IEnvironmentHandler
    {
        IClientHandler Clients { get; }

        /*
		 * Resource groups
		 */
        void InsertResourceGroup(Guid token, string name, Guid storageProvider, string connectionString);
        void UpdateResourceGroup(IResourceGroup item, string name, Guid storageProvider, string connectionString);
        void DeleteResourceGroup(IResourceGroup item);

        List<IServerResourceGroup> QueryResourceGroups();
        IServerResourceGroup SelectResourceGroup(Guid token);
        /*
		 * Environment variables
		 */
        void UpdateEnvironmentVariable(string name, string value);
        IEnvironmentVariable SelectEnvironmentVariable(string name);
        List<IEnvironmentVariable> QueryEnvironmentVariables();
        void DeleteEnvironmentVariable(string name);
    }
}
