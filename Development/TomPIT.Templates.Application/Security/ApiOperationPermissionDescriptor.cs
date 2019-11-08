using System;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT.MicroServices.Security
{
	public class ApiOperationPermissionDescriptor : IPermissionDescriptor
	{
		public string Id => "Api operation";

		public IPermissionDescription GetDescription(ITenant tenant, Guid evidence, string component)
		{
			var api = tenant.GetService<IComponentService>().SelectConfiguration(new Guid(component)) as IApiConfiguration;

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
