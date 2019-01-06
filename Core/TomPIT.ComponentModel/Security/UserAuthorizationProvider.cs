using System;
using System.Collections.Generic;
using TomPIT.Runtime;

namespace TomPIT.Security
{
	internal class UserAuthorizationProvider : IAuthorizationProvider
	{
		public string Id => "Users";

		public AuthorizationProviderResult Authorize(IApplicationContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state)
		{
			if (e.User != permission.Evidence)
				return AuthorizationProviderResult.NotHandled;

			switch (permission.Value)
			{
				case PermissionValue.NotSet:
					return AuthorizationProviderResult.NotHandled;
				case PermissionValue.Allow:
					return AuthorizationProviderResult.Success;
				case PermissionValue.Deny:
					return AuthorizationProviderResult.Fail;
				default:
					throw new NotSupportedException();
			}
		}

		public AuthorizationProviderResult PreAuthorize(IApplicationContext context, AuthorizationArgs e, Dictionary<string, object> state)
		{
			return AuthorizationProviderResult.NotHandled;
		}

		public List<IPermissionSchemaDescriptor> QueryDescriptors(IApplicationContext context)
		{
			var ctx = context.GetServerContext();
			var users = ctx.GetService<IUserService>().Query();
			var r = new List<IPermissionSchemaDescriptor>();

			foreach (var i in users)
			{
				r.Add(new SchemaDescriptor
				{
					Id = i.Token,
					Title = i.DisplayName(),
					Description = i.Email
				});
			}

			return r;
		}

	}
}
