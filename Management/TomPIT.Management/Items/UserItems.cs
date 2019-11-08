using System;
using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.Security;

namespace TomPIT.Management.Items
{
	internal class UserItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var u = element.Environment.Context.Tenant.GetService<IUserService>().Query();

			if (!IsRequired(element))
				items.Add(new ItemDescriptor(SR.DevLiDefault, Guid.Empty.ToString()));

			foreach (var i in u)
				items.Add(new ItemDescriptor(i.DisplayName(), i.Token.ToString()));
		}
	}
}
