using System;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	public abstract class ComponentPermissionDescriptor : IPermissionDescriptor
	{
		public abstract string Id { get; }

		public IPermissionDescription GetDescription(ISysConnection connection, Guid evidence, string component)
		{
			var ms = connection.GetService<IComponentService>().SelectComponent(evidence);

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
