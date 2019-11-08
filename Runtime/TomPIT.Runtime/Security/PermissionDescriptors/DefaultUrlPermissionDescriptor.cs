using System;
using TomPIT.Connectivity;

namespace TomPIT.Security.PermissionDescriptors
{
	public class DefaultUrlPermissionDescriptor : IPermissionDescriptor
	{
		public string Id => "Default Url";

		public IPermissionDescription GetDescription(ITenant tenant, Guid evidence, string component)
		{
			return new PermissionDescription
			{
				Id = evidence,
				Title = component
			};
		}
	}
}
