using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using TomPIT.Security;

namespace TomPIT.Proxy.Management
{
    public interface ISecurityManagementController
    {
        Task<PermissionValue> SetPermission(string evidence, string schema, string claim, string descriptor, string primaryKey, Guid resourceGroup, string component);
        void Reset(string schema, string claim, string primaryKey, string descriptor);
        ImmutableList<IMembership> QueryMembership(Guid role);
        void InsertMembership(Guid user, Guid role);
        void DeleteMembership(Guid user, Guid role);
        Guid InsertAuthenticationToken(Guid resourceGroup, Guid user, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status,
            DateTime validFrom, DateTime validTo, TimeSpan startTime, TimeSpan endTime, string ipRestrictions, string name, string description);
        void UpdateAuthenticationToken(Guid token, Guid user, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status,
            DateTime validFrom, DateTime validTo, TimeSpan startTime, TimeSpan endTime, string ipRestrictions, string name, string description);
        void DeleteAuthenticationToken(Guid token);
    }
}
