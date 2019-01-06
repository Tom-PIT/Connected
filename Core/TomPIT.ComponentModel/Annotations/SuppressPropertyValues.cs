using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SuppressPropertyValues : Attribute
	{
		public object[] Values { get; private set; }

		public SuppressPropertyValues() { }
		public SuppressPropertyValues(params object[] values) { Values = values; }
	}
}
