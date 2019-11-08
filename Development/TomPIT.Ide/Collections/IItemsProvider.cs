using System.Collections.Generic;

namespace TomPIT.Ide.Collections
{
	public interface IItemsProvider
	{
		List<IItemDescriptor> QueryDescriptors(ItemsDescriptorArgs e);
	}
}
