using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace TomPIT.Security
{
    public interface IPermissionService
    {
        [Obsolete("Please use async method")]
        PermissionValue Toggle(string claim, string schema, string evidence, string primaryKey, string permissionDescriptor);
        Task<PermissionValue> ToggleAsync(string claim, string schema, string evidence, string primaryKey, string permissionDescriptor);
        void Reset(string claim, string schema, string primaryKey, string descriptor);
        void Reset(string primaryKey);
        ImmutableList<IPermission> Query(string descriptor, string primaryKey);
        ImmutableList<IPermission> Query(string descriptor, Guid user);
    }
}
