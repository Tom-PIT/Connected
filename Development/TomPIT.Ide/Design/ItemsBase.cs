using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TomPIT.Dom;

namespace TomPIT.Design
{
	public abstract class ItemsBase : IItemsProvider
	{
		public List<IItemDescriptor> QueryDescriptors(ItemsDescriptorArgs e)
		{
			Property = e.Property;
			var r = new List<IItemDescriptor>();

			OnQueryDescriptors(e.Element, r);

			return r;
		}

		protected PropertyInfo Property { get; private set; }
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

		protected bool IsRequired(IDomElement element)
		{
			if (Property == null)
				return false;

			return Property.FindAttribute<RequiredAttribute>() != null;
		}
	}
}
