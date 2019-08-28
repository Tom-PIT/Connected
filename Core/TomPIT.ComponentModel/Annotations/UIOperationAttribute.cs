using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class UIOperationAttribute : Attribute
	{
		public UIOperationAttribute(string partialName)
		{
			PartialName = partialName;
		}

		public string PartialName { get; }
	}
}