using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using TomPIT.Middleware;
using TomPIT.Navigation;

namespace TomPIT.Security
{
	public interface IAuthorizationService
	{
		IAuthorizationResult Authorize(IMiddlewareContext context, AuthorizationArgs e);
		bool Demand(Guid user, Guid role);

		IClientAuthenticationResult Authenticate(string user, string password);
		IClientAuthenticationResult AuthenticateByPin(string user, string pin);
		IClientAuthenticationResult Authenticate(string authenticationToken);
		IClientAuthenticationResult Authenticate(Guid authenticationToken);
		bool IsInRole(Guid user, string role);
		void RegisterAuthorizationProvider(IAuthorizationProvider provider);
		ImmutableList<IAuthorizationProvider> QueryProviders();
		void RegisterDescriptor(IPermissionDescriptor descriptor);
		ImmutableList<IPermissionDescriptor> QueryDescriptors();

		PermissionValue GetPermissionValue(string evidence, string schema, string claim, string descriptor);
		PermissionValue GetPermissionValue(string evidence, string schema, string claim, string descriptor, string primaryKey);
		void RegisterAuthenticationProvider(string id, IAuthenticationProvider provider);
		void Authorize(ISiteMapContainer container);

		ImmutableList<IMembership> QueryMembership(Guid user);
		ImmutableList<IMembership> QueryMembershipForRole(Guid role);

		[Obsolete("Please use async method")]
		void AuthorizePolicies(IMiddlewareContext context, object instance);
		[Obsolete("Please use async method")]
		void AuthorizePolicies(IMiddlewareContext context, object instance, string method);

		Task AuthorizePoliciesAsync(IMiddlewareContext context, object instance);
		Task AuthorizePoliciesAsync(IMiddlewareContext context, object instance, string method);
	}
}
