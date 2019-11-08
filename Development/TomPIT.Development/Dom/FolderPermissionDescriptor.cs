using System;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT.Dom
{
	internal class FolderPermissionDescriptor : IPermissionDescriptor
	{
		public string Id => "Folder";

		public IPermissionDescription GetDescription(ITenant tenant, Guid evidence, string component)
		{
			var ms = tenant.GetService<IComponentService>().SelectFolder(evidence);

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
