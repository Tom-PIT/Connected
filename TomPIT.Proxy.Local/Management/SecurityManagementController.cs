using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using TomPIT.Proxy.Management;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class SecurityManagementController : ISecurityManagementController
{
    public void DeleteAuthenticationToken(Guid token)
    {
        DataModel.AuthenticationTokens.Delete(token);
    }

    public void DeleteMembership(Guid user, Guid role)
    {
        DataModel.Membership.Delete(user, role);
    }

    public Guid InsertAuthenticationToken(Guid resourceGroup, Guid user, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo, TimeSpan startTime, TimeSpan endTime, string ipRestrictions, string name, string description)
    {
        return DataModel.AuthenticationTokens.Insert(resourceGroup, user, name, description, key, claims, status, validFrom, validTo, startTime, endTime, ipRestrictions);
    }

    public void InsertMembership(Guid user, Guid role)
    {
        DataModel.Membership.Insert(user, role);
    }

    public ImmutableList<IMembership> QueryMembership(Guid role)
    {
        return DataModel.Membership.QueryForRole(role);
    }

    public void Reset(string schema, string claim, string primaryKey, string descriptor)
    {
        DataModel.Permissions.Reset(claim, schema, primaryKey, descriptor);
    }

    public async Task<PermissionValue> SetPermission(string evidence, string schema, string claim, string descriptor, string primaryKey, Guid resourceGroup, string component)
    {
        var p = DataModel.Permissions.Select(evidence, schema, claim, primaryKey, descriptor);

        if (p is null)
        {
            await DataModel.Permissions.Insert(resourceGroup, evidence, schema, claim, descriptor, primaryKey, PermissionValue.Allow, component);

            return PermissionValue.Allow;
        }
        else
        {
            var v = PermissionValue.NotSet;

            switch (p.Value)
            {
                case PermissionValue.NotSet:
                    v = PermissionValue.Allow;
                    break;
                case PermissionValue.Allow:
                    v = PermissionValue.Deny;
                    break;
                case PermissionValue.Deny:
                    v = PermissionValue.NotSet;
                    break;
            }

            if (v == PermissionValue.NotSet)
                DataModel.Permissions.Delete(evidence, schema, claim, primaryKey, descriptor);
            else
                DataModel.Permissions.Update(evidence, schema, claim, primaryKey, descriptor, v);

            return v;
        }

    }

    public void UpdateAuthenticationToken(Guid token, Guid user, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo, TimeSpan startTime, TimeSpan endTime, string ipRestrictions, string name, string description)
    {
        DataModel.AuthenticationTokens.Update(token, user, name, description, key, claims, status, validFrom, validTo, startTime, endTime, ipRestrictions);
    }
}
