using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Environment;
using TomPIT.Ide.Collections;

namespace TomPIT.Management.Items
{
	internal class ResourceGroupsCollection : ItemsBase
	{
		public const string ResourceGroup = "{FB13EB74-5E7C-4E8D-93CC-906FDA1DAE9C}";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Resource group", ResourceGroup, typeof(IResourceGroup)));
		}
	}
}
