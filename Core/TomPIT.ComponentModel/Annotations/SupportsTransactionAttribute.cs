using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SupportsTransactionAttribute : Attribute
	{
	}
}
