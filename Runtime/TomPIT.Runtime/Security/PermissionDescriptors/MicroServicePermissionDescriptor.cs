using System;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Security.PermissionDescriptors
{
	public class MicroServicePermissionDescriptor : IPermissionDescriptor
	{
		public string Id => "Micro service";

		public IPermissionDescription GetDescription(ITenant tenant, Guid evidence, string component)
		{
			var ms = tenant.GetService<IMicroServiceService>().Select(evidence);

			if (ms == null)
				return null;

			return new PermissionDescription
			{
				Id = evidence,
				Title = ms.Name
			};
		}
	}
}
