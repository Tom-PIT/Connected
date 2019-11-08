using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;

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
