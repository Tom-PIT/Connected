using System;

namespace TomPIT.Design
{
	public class MicroServiceTemplateDescriptor : IMicroServiceTemplateDescriptor
	{
		public Guid Token { get; set; }
		public string Name { get; set; }
	}
}
