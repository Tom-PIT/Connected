using System.Collections.Generic;
using TomPIT.Application.QA;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Design.Items
{
	internal class TestScenariosCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Scenario", "Scenario", typeof(TestScenario)));
		}
	}
}
