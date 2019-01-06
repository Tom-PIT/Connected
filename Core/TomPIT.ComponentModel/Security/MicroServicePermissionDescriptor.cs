using System;
using TomPIT.ComponentModel;
using TomPIT.Net;

namespace TomPIT.Security
{
	public class MicroServicePermissionDescriptor : IPermissionDescriptor
	{
		public string Id => "Micro service";

		public IPermissionDescription GetDescription(ISysContext context, Guid evidence)
		{
			var ms = context.GetService<IMicroServiceService>().Select(evidence);

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
