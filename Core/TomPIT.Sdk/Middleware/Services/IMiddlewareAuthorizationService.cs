using System;
using System.Threading.Tasks;
using TomPIT.Annotations;

namespace TomPIT.Middleware.Services
{
    public interface IMiddlewareAuthorizationService
    {
        [Obsolete("Please use async method")]
        bool Authorize(object claim, object primaryKey, string permissionDescriptor);
        [Obsolete("Please use async method")]
        bool Authorize(object claim, object primaryKey, string permissionDescriptor, Guid user);
        [Obsolete("Please use async method")]
        void Allow(object claim, object primaryKey, string permissionDescriptor);
        [Obsolete("Please use async method")]
        void Allow(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence);
        [Obsolete("Please use async method")]
        void Deny(object claim, object primaryKey, string permissionDescriptor);
        [Obsolete("Please use async method")]
        void Deny(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence);

        Task<bool> AuthorizeAsync(object claim, object primaryKey, string permissionDescriptor);
        Task<bool> AuthorizeAsync(object claim, object primaryKey, string permissionDescriptor, Guid user);

        Task AllowAsync(object claim, object primaryKey, string permissionDescriptor);
        Task AllowAsync(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence);

        Task DenyAsync(object claim, object primaryKey, string permissionDescriptor);
        Task DenyAsync(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence);

        T CreatePolicy<T>() where T : AuthorizationPolicyAttribute;
    }
}
