using System;
using System.Collections.Generic;
using TomPIT.Middleware;
using TomPIT.Navigation;

namespace TomPIT.Security
{
	public interface IAuthorizationService
	{
		IAuthorizationResult Authorize(IMiddlewareContext context, AuthorizationArgs e);
		bool Demand(Guid user, Guid role);

		IClientAuthenticationResult Authenticate(string user, string password);
		IClientAuthenticationResult Authenticate(string authenticationToken);
		bool IsInRole(Guid user, string role);
		void RegisterProvider(IAuthorizationProvider provider);
		List<IAuthorizationProvider> QueryProviders();
		void RegisterDescriptor(IPermissionDescriptor descriptor);
		List<IPermissionDescriptor> QueryDescriptors();

		PermissionValue GetPermissionValue(Guid evidence, string schema, string claim);
		PermissionValue GetPermissionValue(Guid evidence, string schema, string claim, string descriptor, string primaryKey);
		void RegisterAuthenticationProvider(IAuthenticationProvider provider);
		void Authorize(ISiteMapContainer container);

		List<IMembership> QueryMembership(Guid user);
		List<IMembership> QueryMembershipForRole(Guid role);
	}
}
