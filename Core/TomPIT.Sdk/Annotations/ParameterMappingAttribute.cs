using System;

namespace TomPIT.Annotations
{
	public sealed class ParameterMappingAttribute : Attribute
	{
		public string Name { get; set; }
	}
}
