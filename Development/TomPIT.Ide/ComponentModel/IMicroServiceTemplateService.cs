using System;
using System.Collections.Immutable;

namespace TomPIT.Ide.ComponentModel
{
	public interface IMicroServiceTemplateService
	{
		ImmutableList<IMicroServiceTemplate> Query();
		void Register(IMicroServiceTemplate template);
		IMicroServiceTemplate Select(Guid template);
	}
}
