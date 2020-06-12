using System;

namespace TomPIT.Annotations
{
	public sealed class PrimaryKeyAttribute : Attribute
	{
		public bool Identity { get; set; } = true;
	}
}
