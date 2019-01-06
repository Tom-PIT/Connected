using System;
using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Items
{
	internal class StorageProviderItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var u = element.Environment.Context.GetServerContext().CreateUrl("StorageManagement", "QueryStorageProviders");
			var ds = element.Environment.Context.GetServerContext().Connection.Get<List<ClientStorageProvider>>(u);

			items.Add(new ItemDescriptor(SR.DevLiDefault, Guid.Empty.ToString()));

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Token.ToString()));
		}
	}
}
