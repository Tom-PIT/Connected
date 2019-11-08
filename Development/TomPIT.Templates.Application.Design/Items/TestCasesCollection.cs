using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.Quality;

namespace TomPIT.MicroServices.Design.Items
{
	internal class TestCasesCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Test case", "TestCase", typeof(TestCase)));
		}
	}
}
