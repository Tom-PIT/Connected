using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Environment;
using TomPIT.Security;

namespace TomPIT.SysDb.Security
{
    public interface IPermissionHandler
    {
        Task Insert(IResourceGroup resourceGroup, string evidence, string schema, string claim, string descriptor, string primaryKey, PermissionValue value, string component);
        void Update(IPermission permission, PermissionValue value);

        void Delete(IPermission permission);

        List<IPermission> Query();

        IPermission Select(string evidence, string schema, string claim, string primaryKey, string descriptor);
    }
}
