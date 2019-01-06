using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ResourceAttribute : Attribute
	{
		public ResourceAttribute(string value)
		{
			Value = value;
		}

		public string Value { get; }

		public string GetValue()
		{
			var r = SR.ResourceManager.GetString(Value);

			if (!string.IsNullOrWhiteSpace(r))
				return r;

			return Value;
		}
	}
}
