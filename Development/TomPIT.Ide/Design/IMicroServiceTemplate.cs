using System;
using System.Collections.Generic;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public interface IMicroServiceTemplate : IMicroServiceTemplateDescriptor
	{
		List<IDomElement> QueryDomRoot(IEnvironment environment, IDomElement parent, Guid microService);
		List<IItemDescriptor> QueryDescriptors(IDomElement parent, string category);
	}
}
