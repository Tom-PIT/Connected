using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	public class UrlPermissionDescriptor : IPermissionDescriptor
	{
		public string Id => "Url";

		public IPermissionDescription GetDescription(ISysConnection connection, Guid evidence, string component)
		{
			return new PermissionDescription
			{
				Id = evidence,
				Title = component
			};
		}
	}
}
