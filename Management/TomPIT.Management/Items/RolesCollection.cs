using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.Security;

namespace TomPIT.Management.Items
{
	internal class RolesCollection : ItemsBase
	{
		public const string Role = "{790051A5-6CE8-4E1F-A9FF-058464394099}";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Role", Role, typeof(IRole)));
		}
	}
}
