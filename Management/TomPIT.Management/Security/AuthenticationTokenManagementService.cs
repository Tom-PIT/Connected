using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
    internal class AuthenticationTokenManagementService : TenantObject, IAuthenticationTokenManagementService
    {
        public AuthenticationTokenManagementService(ITenant tenant) : base(tenant)
        {

        }

        public void Delete(Guid token)
        {
            Instance.SysProxy.Management.Security.DeleteAuthenticationToken(token);

            if (Tenant.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
                n.NotifyAuthenticationTokenRemoved(this, new AuthenticationTokenEventArgs(token));
        }

        public Guid Insert(Guid resourceGroup, Guid user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status,
            DateTime validFrom, DateTime validTo, TimeSpan startTime, TimeSpan endTime, string ipRestrictions)
        {
            var id = Instance.SysProxy.Management.Security.InsertAuthenticationToken(resourceGroup, user, key, claims, status, validFrom, validTo, startTime, endTime, ipRestrictions, name, description);

            if (Tenant.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
                n.NotifyAuthenticationTokenChanged(this, new AuthenticationTokenEventArgs(id));

            return id;
        }

        public List<IAuthenticationToken> Query(string resourceGroup)
        {
            return Instance.SysProxy.Security.QueryAuthenticationTokens(new List<string> { resourceGroup }).ToList();
        }

        public IAuthenticationToken Select(Guid token)
        {
            return Instance.SysProxy.Security.SelectAuthenticationToken(token);
        }

        public void Update(Guid token, Guid user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo,
            TimeSpan startTime, TimeSpan endTime, string ipRestrictions)
        {
            Instance.SysProxy.Management.Security.UpdateAuthenticationToken(token, user, key, claims, status, validFrom, validTo, startTime, endTime, ipRestrictions, name, description);

            if (Tenant.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
                n.NotifyAuthenticationTokenChanged(this, new AuthenticationTokenEventArgs(token));
        }
    }
}
