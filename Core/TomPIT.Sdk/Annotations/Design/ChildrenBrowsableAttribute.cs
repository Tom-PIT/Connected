using System;

namespace TomPIT.Annotations.Design
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