using System;

namespace TomPIT.Design
{
	public interface IMicroServiceTemplateDescriptor
	{
		Guid Token { get; }
		string Name { get; }
	}
}
