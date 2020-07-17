using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;
using TomPIT.Security.PermissionDescriptors;

namespace TomPIT.Security.AuthorizationProviders
{
	internal class PolicyAuthorizationProvider : IAuthorizationProvider
	{
		public string Id => "Policies";

		public AuthorizationProviderResult Authorize(IMiddlewareContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state)
		{
			return AuthorizationProviderResult.NotHandled;
		}

		public AuthorizationProviderResult PreAuthorize(IMiddlewareContext context, AuthorizationArgs e, Dictionary<string, object> state)
		{
			return AuthorizationProviderResult.NotHandled;
		}

		public List<IPermissionSchemaDescriptor> QueryDescriptors(IMiddlewareContext context)
		{
			var descriptors = context.Tenant.GetService<IComponentService>().QueryComponents(Shell.GetConfiguration<IClientSys>().ResourceGroups, ComponentCategories.PermissionDescriptor);
			var result = new List<IPermissionSchemaDescriptor>();

			foreach (var descriptor in descriptors)
			{
				var ms = context.Tenant.GetService<IMicroServiceService>().Select(descriptor.MicroService);

				result.Add(new SchemaDescriptor
				{
					Id = descriptor.Token.ToString(),
					Title = descriptor.Name,
					Description = ms.Name
				});
			}

			return result;
		}
	}
}
