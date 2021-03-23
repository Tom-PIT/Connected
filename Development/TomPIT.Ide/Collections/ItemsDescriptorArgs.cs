using System;
using System.Reflection;
using TomPIT.Design.Ide.Dom;

namespace TomPIT.Ide.Collections
{
	public class ItemsDescriptorArgs : EventArgs
	{
		public ItemsDescriptorArgs(IDomElement element, PropertyInfo property)
		{
			Element = element;
			Property = property;
		}

		public IDomElement Element { get; }
		public PropertyInfo Property { get; }
	}
}
