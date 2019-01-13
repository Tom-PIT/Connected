using System;
using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.Security
{
	public interface IAuthorizationService
	{
		IAuthorizationResult Authorize(IExecutionContext context, AuthorizationArgs e);
		bool Demand(Guid user, Guid role);

		IClientAuthenticationResult Authenticate(string user, string password);
		IClientAuthenticationResult Authenticate(string authToken);
		bool IsInRole(Guid user, string role);
		void RegisterProvider(IAuthorizationProvider provider);
		List<IAuthorizationProvider> QueryProviders();
		void RegisterDescriptor(IPermissionDescriptor descriptor);
		List<IPermissionDescriptor> QueryDescriptors();

		PermissionValue GetPermissionValue(Guid evidence, string schema, string claim);
		void RegisterAuthenticationProvider(IAuthenticationProvider provider);
	}
}
