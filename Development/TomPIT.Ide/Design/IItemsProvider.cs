using System.Collections.Generic;
using TomPIT.Dom;

namespace TomPIT.Design
{
	public interface IItemsProvider
	{
		List<IItemDescriptor> QueryDescriptors(ItemsDescriptorArgs e);
	}
}
