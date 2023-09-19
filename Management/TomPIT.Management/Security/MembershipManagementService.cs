using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
    internal class MembershipManagementService : TenantObject, IMembershipManagementService
    {
        public MembershipManagementService(ITenant tenant) : base(tenant)
        {

        }

        public void Delete(Guid user, Guid role)
        {
            Instance.SysProxy.Management.Security.DeleteMembership(user, role);

            if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
                n.NotifyMembershipRemoved(this, new MembershipEventArgs(user, role));
        }

        public void Insert(Guid user, Guid role)
        {
            Instance.SysProxy.Management.Security.InsertMembership(user, role);

            if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
                n.NotifyMembershipAdded(this, new MembershipEventArgs(user, role));
        }

        public List<IMembership> Query(Guid role)
        {
            return Instance.SysProxy.Management.Security.QueryMembership(role).ToList();
        }
    }
}
