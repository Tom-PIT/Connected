using System;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT.Application.Security
{
	public class ApiOperationPermissionDescriptor : IPermissionDescriptor
	{
		public string Id => "Api operation";

		public IPermissionDescription GetDescription(ISysConnection connection, Guid evidence, string component)
		{
			var api = connection.GetService<IComponentService>().SelectConfiguration(component.AsGuid()) as IApi;

			if (api == null)
				return null;

			var op = api.Operations.FirstOrDefault(f => f.Id == evidence);

			if (op == null)
				return null;

			return new PermissionDescription
			{
				Id = evidence,
				Title = op.Name
			};
		}
	}
}
