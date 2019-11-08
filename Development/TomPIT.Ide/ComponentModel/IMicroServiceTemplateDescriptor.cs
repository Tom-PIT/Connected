using System;

namespace TomPIT.Ide.ComponentModel
{
	public interface IMicroServiceTemplateDescriptor
	{
		Guid Token { get; }
		string Name { get; }
	}
}
