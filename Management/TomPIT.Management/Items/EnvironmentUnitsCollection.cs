using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Environment;

namespace TomPIT.Items
{
	internal class EnvironmentUnitsCollection : ItemsBase
	{
		public const string Units = "{49336F43-CC15-42C0-8B0B-C976C382EB4B}";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Environment unit", Units, typeof(IEnvironmentUnit)));
		}
	}
}
