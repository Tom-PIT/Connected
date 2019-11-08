using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class CreateAttribute : Attribute
	{
		public CreateAttribute(string prefix)
		{
			Prefix = prefix;
		}

		public CreateAttribute(string prefix, string propertyName)
		{
			Prefix = prefix;
			PropertyName = propertyName;
		}

		public string Prefix { get; }
		public string PropertyName { get; }
	}
}
