using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class AuthorizationProxyAttribute : Attribute
	{
		public Type Type { get; set; }
	}
}
