using System;

namespace TomPIT.Annotations
{
	public sealed class IndexAttribute : Attribute
	{
		public bool Unique { get; set; }
		public string Group { get; set; }
	}
}
