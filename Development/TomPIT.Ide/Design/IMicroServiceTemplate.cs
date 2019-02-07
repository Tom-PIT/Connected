using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public interface IMicroServiceTemplate : IMicroServiceTemplateDescriptor
	{
		List<IItemDescriptor> ProvideAddItems(IDomElement parent);
		IComponent References(IEnvironment environment, Guid microService);
		List<string> GetApplicationParts();
	}
}
