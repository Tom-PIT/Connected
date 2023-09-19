using System;
using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;

namespace TomPIT.Management.Items
{
    internal class StorageProviderItems : ItemsBase
    {
        protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
        {
            var ds = Instance.SysProxy.Management.Storage.QueryStorageProviders();

            items.Add(new ItemDescriptor(SR.DevLiDefault, Guid.Empty.ToString()));

            foreach (var i in ds)
                items.Add(new ItemDescriptor(i.Name, i.Token.ToString()));
        }
    }
}
