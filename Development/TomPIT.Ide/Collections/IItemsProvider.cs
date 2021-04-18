using System.Collections.Generic;
using TomPIT.Design.Ide;

namespace TomPIT.Ide.Collections
{
	public interface IItemsProvider
	{
		List<IItemDescriptor> QueryDescriptors(ItemsDescriptorArgs e);
	}
}
