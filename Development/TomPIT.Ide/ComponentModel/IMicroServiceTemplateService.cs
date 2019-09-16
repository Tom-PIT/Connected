using System;
using System.Collections.Generic;

namespace TomPIT.Ide.ComponentModel
{
	public interface IMicroServiceTemplateService
	{
		List<IMicroServiceTemplate> Query();
		void Register(IMicroServiceTemplate template);
		IMicroServiceTemplate Select(Guid template);
	}
}
