using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Ide.ComponentModel
{
	internal class MicroServiceTemplateService : ClientRepository<IMicroServiceTemplate, Guid>, IMicroServiceTemplateService
	{
		public MicroServiceTemplateService(ITenant tenant) : base(tenant, "microservicetemplate")
		{

		}

		public List<IMicroServiceTemplate> Query()
		{
			return All();
		}

		public void Register(IMicroServiceTemplate template)
		{
			Set(template.Token, template, TimeSpan.Zero);
		}

		public IMicroServiceTemplate Select(Guid template)
		{
			return Get(template);
		}
	}
}
