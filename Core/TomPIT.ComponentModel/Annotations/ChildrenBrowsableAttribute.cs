using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class ChildrenBrowsableAttribute : Attribute
	{
		public ChildrenBrowsableAttribute() { }

		public ChildrenBrowsableAttribute(bool browsable)
		{
			Browsable = browsable;
		}

		public bool Browsable { get; }
	}
}