using System;

namespace TomPIT.Annotations.Models
{
	public sealed class IndexAttribute : Attribute
	{
		public bool Unique { get; set; }
		public string Group { get; set; }
	}
}
