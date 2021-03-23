using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;

namespace TomPIT.Management.Items
{
	internal class BigDataNodesCollection : ItemsBase
	{
		public const string Token = "Node";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Node", Token, typeof(INode)));
		}
	}
}
