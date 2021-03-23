using System;
using TomPIT.Design;

namespace TomPIT.Ide.ComponentModel
{
	public class MicroServiceTemplateDescriptor : IMicroServiceTemplateDescriptor
	{
		public Guid Token { get; set; }
		public string Name { get; set; }
	}
}
