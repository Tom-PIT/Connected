using System;
using TomPIT.ComponentModel;
using TomPIT.Net;

namespace TomPIT.Security
{
	public abstract class ComponentPermissionDescriptor : IPermissionDescriptor
	{
		public abstract string Id { get; }

		public IPermissionDescription GetDescription(ISysContext context, Guid evidence)
		{
			var ms = context.GetService<IComponentService>().SelectComponent(evidence);

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
