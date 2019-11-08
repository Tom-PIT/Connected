using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class DomSignatureAttribute : Attribute
	{
		public string Signature { get; set; }
	}
}
