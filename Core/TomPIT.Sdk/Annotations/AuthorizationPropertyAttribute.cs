using System;

namespace TomPIT.Annotations
{
	[Obsolete("Please use ProxyPropertyAttrbute instead.")]
	public sealed class AuthorizationPropertyAttribute : Attribute
	{
		public AuthorizationPropertyAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }
	}
}
