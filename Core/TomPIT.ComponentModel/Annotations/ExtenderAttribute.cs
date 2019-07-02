using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ExtenderAttribute : Attribute
	{
		public ExtenderAttribute(Type extender)
		{
			Extender = extender;
		}

		public Type Extender { get; }
	}
}
