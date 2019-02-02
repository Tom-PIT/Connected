using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public interface IMicroServiceTemplate : IMicroServiceTemplateDescriptor
	{
		List<IDomElement> QueryDomRoot(IEnvironment environment, IDomElement parent, Guid microService);
		List<IItemDescriptor> ProvideAddItems(IDomElement parent);
		IComponent References(IEnvironment environment, Guid microService);
	}
}
