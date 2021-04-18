using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	[Obsolete("Transactions are implicit. No need to use this attribute.")]
	public class SupportsTransactionAttribute : Attribute
	{
	}
}
