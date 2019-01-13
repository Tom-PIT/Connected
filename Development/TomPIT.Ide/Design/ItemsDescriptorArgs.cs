using System;
using System.Reflection;
using TomPIT.Dom;

namespace TomPIT.Design
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
