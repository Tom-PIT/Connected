using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using TomPIT.Proxy.Management;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote.Management;
internal class SecurityManagementController : ISecurityManagementController
{
    private const string Controller = "SecurityManagement";

    public void DeleteAuthenticationToken(Guid token)
    {
        Connection.Post(Connection.CreateUrl(Controller, "DeleteAuthenticationToken"), new
        {
            token
        });
    }

    public void DeleteMembership(Guid user, Guid role)
    {
        Connection.Post(Connection.CreateUrl(Controller, "DeleteMembership"), new
        {
            user,
            role
        });
    }

    public Guid InsertAuthenticationToken(Guid resourceGroup, Guid user, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo, TimeSpan startTime, TimeSpan endTime, string ipRestrictions, string name, string description)
    {
        return Connection.Post<Guid>(Connection.CreateUrl(Controller, "InsertAuthenticationToken"), new
        {
            resourceGroup,
            user,
            key,
            claims,
            status,
            validFrom,
            validTo,
            startTime,
            endTime,
            ipRestrictions,
            name,
            description
        });
    }

    public void InsertMembership(Guid user, Guid role)
    {
        Connection.Post(Connection.CreateUrl(Controller, "InsertMembership"), new
        {
            user,
            role
        });
    }

    public ImmutableList<IMembership> QueryMembership(Guid role)
    {
        return Connection.Get<List<Membership>>(Connection.CreateUrl(Controller, "QueryMembership").AddParameter("role", role)).ToImmutableList<IMembership>();
    }

    public void Reset(string schema, string claim, string primaryKey, string descriptor)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Reset"), new
        {
            schema,
            claim,
            primaryKey,
            descriptor
        });
    }

    public async Task<PermissionValue> SetPermission(string evidence, string schema, string claim, string descriptor, string primaryKey, Guid resourceGroup, string component)
    {
        return Connection.Post<PermissionValue>(Connection.CreateUrl(Controller, "SetPermission"), new
        {
            claim,
            schema,
            descriptor,
            primaryKey,
            evidence,
            resourceGroup,
            component
        });
    }

    public void UpdateAuthenticationToken(Guid token, Guid user, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo, TimeSpan startTime, TimeSpan endTime, string ipRestrictions, string name, string description)
    {
        Connection.Post(Connection.CreateUrl(Controller, "UpdateAuthenticationToken"), new
        {
            token,
            user,
            key,
            claims,
            status,
            validFrom,
            validTo,
            startTime,
            endTime,
            ipRestrictions,
            name,
            description
        });
    }
}
