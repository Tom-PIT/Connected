using System;

namespace TomPIT.Annotations
{
	public sealed class AuthorizationPropertyAttribute : Attribute
	{
		public AuthorizationPropertyAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }
	}
}
