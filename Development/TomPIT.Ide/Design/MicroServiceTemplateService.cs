using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Design
{
	internal class MicroServiceTemplateService : ClientRepository<IMicroServiceTemplate, Guid>, IMicroServiceTemplateService
	{
		public MicroServiceTemplateService(ISysConnection connection) : base(connection, "microservicetemplate")
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
