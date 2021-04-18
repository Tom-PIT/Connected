using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Design
{
	internal class MicroServiceTemplateService : ClientRepository<IMicroServiceTemplate, Guid>, IMicroServiceTemplateService
	{
		public MicroServiceTemplateService(ITenant tenant) : base(tenant, "microservicetemplate")
		{

		}

		public ImmutableList<IMicroServiceTemplate> Query()
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
