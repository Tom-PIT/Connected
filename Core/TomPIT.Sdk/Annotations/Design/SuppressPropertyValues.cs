using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SuppressPropertyValues : Attribute
	{
		public object[] Values { get; private set; }

		public SuppressPropertyValues() { }
		public SuppressPropertyValues(params object[] values) { Values = values; }
	}
}
