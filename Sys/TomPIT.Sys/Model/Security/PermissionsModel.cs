using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Caching;
using TomPIT.Environment;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Security
{
    public class PermissionsModel : SynchronizedRepository<IPermission, string>
    {
        public PermissionsModel(IMemoryCache container) : base(container, "permission")
        {
        }

        protected override void OnInitializing()
        {
            var ds = Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Query();

            foreach (var i in ds)
                Set(GenerateKey(i.Evidence, i.Schema, i.Claim, i.PrimaryKey, i.Descriptor), i, TimeSpan.Zero);
        }

        protected override void OnInvalidate(string id)
        {
            var tokens = id.Split('.');

            var r = Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Select(tokens[0], tokens[1], tokens[2], tokens[3], tokens[4]);

            if (r == null)
            {
                Remove(id);
                return;
            }

            Set(id, r, TimeSpan.Zero);
        }

        public IPermission Select(string evidence, string schema, string claim, string primaryKey, string descriptor)
        {
            return Get(GenerateKey(evidence.ToString(), schema, claim, primaryKey, descriptor),
                (f) =>
                {
                    return Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Select(evidence, schema, claim, primaryKey, descriptor);
                });
        }

        public ImmutableList<IPermission> Query(List<string> resourceGroups)
        {
            return Query(resourceGroups.ToResourceGroupList());
        }

        public ImmutableList<IPermission> Query(List<Guid> resourceGroups)
        {
            if (!resourceGroups.Contains(Guid.Empty))
                resourceGroups.Add(Guid.Empty);

            return Where(f => resourceGroups.Any(t => t == f.ResourceGroup));
        }

        public ImmutableList<IPermission> Query(string primaryKey)
        {
            return Where(f => string.Compare(f.PrimaryKey, primaryKey, true) == 0);
        }

        public ImmutableList<IPermission> Query() { return All(); }

        public async Task Insert(Guid resourceGroup, string evidence, string schema, string claim, string descriptor, string primaryKey, PermissionValue value, string component)
        {
            IResourceGroup rg = null;

            if (resourceGroup != Guid.Empty)
            {
                rg = DataModel.ResourceGroups.Select(resourceGroup);

                if (rg == null)
                    throw new SysException(SR.ErrResourceGroupNotFound);
            }

            await Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Insert(rg, evidence, schema, claim, descriptor, primaryKey, value, component);

            var key = GenerateKey(evidence.ToString(), schema, claim, primaryKey, descriptor);

            Refresh(key);
            CachingNotifications.PermissionAdded(resourceGroup, evidence, schema, claim, primaryKey, descriptor);
        }

        public void Update(string evidence, string schema, string claim, string primaryKey, string descriptor, PermissionValue value)
        {
            var p = Select(evidence, schema, claim, primaryKey, descriptor);

            if (p == null)
                throw new SysException(SR.ErrPermissionNotFound);

            Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Update(p, value);

            var key = GenerateKey(evidence.ToString(), schema, claim, primaryKey, descriptor);

            Refresh(key);
            CachingNotifications.PermissionChanged(p.ResourceGroup, evidence, schema, claim, primaryKey, p.Descriptor);
        }

        public void Delete(string evidence, string schema, string claim, string primaryKey, string descriptor)
        {
            var p = Select(evidence, schema, claim, primaryKey, descriptor);

            if (p == null)
                throw new SysException(SR.ErrPermissionNotFound);

            Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Delete(p);

            var key = GenerateKey(evidence.ToString(), schema, claim, primaryKey, descriptor);

            Remove(key);
            CachingNotifications.PermissionRemoved(p.ResourceGroup, evidence, schema, claim, primaryKey, p.Descriptor);
        }

        public void Reset(string claim, string schema, string primaryKey, string descriptor)
        {
            var permissions = Where(f => string.Compare(f.PrimaryKey, primaryKey, true) == 0);

            foreach (var permission in permissions)
            {
                if (!string.IsNullOrEmpty(claim) && string.Compare(permission.Claim, claim, true) != 0)
                    continue;

                if (!string.IsNullOrEmpty(schema) && string.Compare(permission.Schema, schema, true) != 0)
                    continue;

                if (!string.IsNullOrEmpty(descriptor) && string.Compare(permission.Descriptor, descriptor, true) != 0)
                    continue;

                Delete(permission.Evidence, permission.Schema, permission.Claim, permission.PrimaryKey, permission.Descriptor);
            }
        }
    }
}