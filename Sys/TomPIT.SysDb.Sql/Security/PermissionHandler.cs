﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Data.Sql;
using TomPIT.Environment;
using TomPIT.Security;
using TomPIT.SysDb.Security;

namespace TomPIT.SysDb.Sql.Security
{
    internal class PermissionHandler : IPermissionHandler
    {
        public void Delete(IPermission permission)
        {
            using var w = new Writer("tompit.permission_del");

            w.CreateParameter("@id", permission.GetId());

            w.Execute();
        }

        public async Task Insert(IResourceGroup resourceGroup, string evidence, string schema, string claim, string descriptor, string primaryKey, PermissionValue value, string component)
        {
            using var w = new Writer("tompit.permission_ins");

            w.CreateParameter("@evidence", evidence);
            w.CreateParameter("@schema", schema);
            w.CreateParameter("@claim", claim);
            w.CreateParameter("@descriptor", descriptor);
            w.CreateParameter("@primary_key", primaryKey);
            w.CreateParameter("@value", value);
            w.CreateParameter("@resource_group", resourceGroup == null ? 0 : resourceGroup.GetId(), true);
            w.CreateParameter("@component", component, true);

            w.Execute();

            await Task.CompletedTask;
        }

        public void Update(IPermission permission, PermissionValue value)
        {
            using var w = new Writer("tompit.permission_upd");

            w.CreateParameter("@id", permission.GetId());
            w.CreateParameter("@value", value);

            w.Execute();
        }

        public List<IPermission> Query()
        {
            using var r = new Reader<Permission>("tompit.permission_que");

            return r.Execute().ToList<IPermission>();
        }

        public IPermission Select(string evidence, string schema, string claim, string primaryKey, string descriptor)
        {
            using var r = new Reader<Permission>("tompit.permission_sel");

            r.CreateParameter("@evidence", evidence, true);
            r.CreateParameter("@schema", schema, true);
            r.CreateParameter("@claim", claim, true);
            r.CreateParameter("@primary_key", primaryKey);
            r.CreateParameter("@descriptor", descriptor);

            return r.ExecuteSingleRow();
        }
    }
}
