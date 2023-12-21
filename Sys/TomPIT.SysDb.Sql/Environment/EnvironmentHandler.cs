using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Environment;
using TomPIT.SysDb.Environment;

namespace TomPIT.SysDb.Sql.Environment
{
    internal class EnvironmentHandler : IEnvironmentHandler
    {
        private IClientHandler _clients = null;

        public IClientHandler Clients
        {
            get
            {
                return _clients ??= new ClientHandler();
            }
        }

        /*
		 * Resource groups
		 */
        public void DeleteResourceGroup(IResourceGroup d)
        {
            using var w = new Writer("tompit.resource_group_del");

            w.CreateParameter("@id", d.GetId());

            w.Execute();
        }

        public void InsertResourceGroup(Guid token, string name, Guid storageProvider, string connectionString)
        {
            using var w = new Writer("tompit.resource_group_ins");

            w.CreateParameter("@token", token);
            w.CreateParameter("@name", name);
            w.CreateParameter("@storage_provider", storageProvider);
            w.CreateParameter("@connection_string", connectionString, true);

            w.Execute();
        }

        public List<IServerResourceGroup> QueryResourceGroups()
        {
            using var r = new Reader<ResourceGroup>("tompit.resource_group_que");

            return r.Execute().ToList<IServerResourceGroup>();
        }

        public IServerResourceGroup SelectResourceGroup(Guid token)
        {
            using var r = new Reader<ResourceGroup>("tompit.resource_group_sel");

            r.CreateParameter("@token", token);

            return r.ExecuteSingleRow();
        }

        public void UpdateResourceGroup(IResourceGroup target, string name, Guid storageProvider, string connectionString)
        {
            using var w = new Writer("tompit.resource_group_upd");

            w.CreateParameter("@id", target.GetId());
            w.CreateParameter("@name", name);
            w.CreateParameter("@storage_provider", storageProvider);
            w.CreateParameter("@connection_string", connectionString, true);

            w.Execute();
        }
        /*
		 * Environment variables
		 */
        public void UpdateEnvironmentVariable(string name, string value)
        {
            using var w = new Writer("tompit.environment_var_mdf");

            w.CreateParameter("@name", name);
            w.CreateParameter("@value", value, true);

            w.Execute();
        }

        public IEnvironmentVariable SelectEnvironmentVariable(string name)
        {
            using var r = new Reader<EnvironmentVariable>("tompit.environment_var_sel");

            r.CreateParameter("@name", name);

            return r.ExecuteSingleRow();
        }

        public List<IEnvironmentVariable> QueryEnvironmentVariables()
        {
            using var r = new Reader<EnvironmentVariable>("tompit.environment_var_que");

            return r.Execute().ToList<IEnvironmentVariable>();
        }

        public void DeleteEnvironmentVariable(string name)
        {
            using var w = new Writer("tompit.environment_var_del");

            w.CreateParameter("@name", name);

            w.Execute();
        }
    }
}
