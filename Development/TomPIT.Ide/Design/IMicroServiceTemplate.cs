using System.Collections.Generic;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public interface IMicroServiceTemplate : IMicroServiceTemplateDescriptor
	{
		List<IDomElement> QueryDomRoot(IEnvironment environment);
		List<IDomElement> QuerySecurityRoot(IDomElement parent);
		List<IItemDescriptor> QueryDescriptors(IDomElement parent, string category);
	}
}
