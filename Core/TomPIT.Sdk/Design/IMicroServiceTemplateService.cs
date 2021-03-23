using System;
using System.Collections.Immutable;

namespace TomPIT.Design
{
	public interface IMicroServiceTemplateService
	{
		ImmutableList<IMicroServiceTemplate> Query();
		void Register(IMicroServiceTemplate template);
		IMicroServiceTemplate Select(Guid template);
	}
}
