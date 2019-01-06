using System;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	public class MicroServicePermissionDescriptor : IPermissionDescriptor
	{
		public string Id => "Micro service";

		public IPermissionDescription GetDescription(ISysConnection connection, Guid evidence)
		{
			var ms = connection.GetService<IMicroServiceService>().Select(evidence);

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
