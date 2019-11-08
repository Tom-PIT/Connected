using System;
using TomPIT.Connectivity;

namespace TomPIT.Security.PermissionDescriptors
{
	public class UrlPermissionDescriptor : IPermissionDescriptor
	{
		public string Id => "Url";

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
