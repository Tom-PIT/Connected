using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class DomSignatureAttribute : Attribute
	{
		public string Signature { get; set; }
	}
}
