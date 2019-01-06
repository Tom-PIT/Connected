using System.Collections.Generic;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public abstract class ItemsBase : IItemsProvider
	{
		public List<IItemDescriptor> QueryDescriptors(IDomElement element)
		{
			var r = new List<IItemDescriptor>();

			OnQueryDescriptors(element, r);

			return r;
		}

		protected virtual void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{

		}

		protected ItemDescriptor Empty()
		{
			return Empty(SR.DevSelect);
		}

		protected ItemDescriptor Empty(string title)
		{
			return Empty(title, string.Empty);
		}

		protected ItemDescriptor Empty(string title, string value)
		{
			return new ItemDescriptor(string.Format("({0})", title), value);
		}
	}
}
