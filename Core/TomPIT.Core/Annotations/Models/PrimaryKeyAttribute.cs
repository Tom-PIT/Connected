using System;

namespace TomPIT.Annotations.Models
{
	public sealed class PrimaryKeyAttribute : Attribute
	{
		public bool Identity { get; set; } = true;
	}
}
