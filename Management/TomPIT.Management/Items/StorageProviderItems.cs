using System;
using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.Middleware;

namespace TomPIT.Management.Items
{
	internal class StorageProviderItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var u = element.Environment.Context.Tenant.CreateUrl("StorageManagement", "QueryStorageProviders");
			var ds = element.Environment.Context.Tenant.Get<List<ClientStorageProvider>>(u);

			items.Add(new ItemDescriptor(SR.DevLiDefault, Guid.Empty.ToString()));

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Token.ToString()));
		}
	}
}
