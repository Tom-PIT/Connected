using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Security;

namespace TomPIT.Items
{
	internal class UsersCollection : ItemsBase
	{
		public const string User = "{A001D11E-5A36-4EC9-801D-9898BDF83992}";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("User", User, typeof(IUser)));
		}
	}
}
