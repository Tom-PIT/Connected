using System;
using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.Security
{
	internal class UserAuthorizationProvider : IAuthorizationProvider
	{
		public string Id => "Users";

		public AuthorizationProviderResult Authorize(IExecutionContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state)
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

		public AuthorizationProviderResult PreAuthorize(IExecutionContext context, AuthorizationArgs e, Dictionary<string, object> state)
		{
			return AuthorizationProviderResult.NotHandled;
		}

		public List<IPermissionSchemaDescriptor> QueryDescriptors(IExecutionContext context)
		{
			var ctx = context.Connection();
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
