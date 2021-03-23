using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.Security;

namespace TomPIT.Management.Items
{
	internal class AuthenticationTokensCollection : ItemsBase
	{
		public const string Token = "{3782A657-3973-4265-A1CC-51A9265F38D5}";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Authentication token", Token, typeof(IAuthenticationToken)));
		}
	}
}
