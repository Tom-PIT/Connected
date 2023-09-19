using System;
using TomPIT.Annotations;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareAuthorizationService
	{
		bool Authorize(object claim, object primaryKey, string permissionDescriptor);
		bool Authorize(object claim, object primaryKey, string permissionDescriptor, Guid user);
		void Allow(object claim, object primaryKey, string permissionDescriptor);
		void Allow(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence);
		void Deny(object claim, object primaryKey, string permissionDescriptor);
		void Deny(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence);

		T CreatePolicy<T>() where T : AuthorizationPolicyAttribute;
	}
}
