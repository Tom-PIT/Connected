using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.Quality;

namespace TomPIT.MicroServices.Design.Items
{
	internal class TestScenariosCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Scenario", "Scenario", typeof(TestScenario)));
		}
	}
}
