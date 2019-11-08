using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ExtenderAttribute : Attribute
	{
		public ExtenderAttribute(Type extender)
		{
			Extender = extender;
		}

		public Type Extender { get; }
	}
}
