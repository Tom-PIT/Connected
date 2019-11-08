using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Data.DataProviders
{
	internal class DataProviderService : ClientRepository<IDataProvider, Guid>, IDataProviderService
	{
		public DataProviderService(ITenant tenant) : base(tenant, "dataprovider")
		{

		}

		public List<IDataProvider> Query()
		{
			return All();
		}

		public void Register(IDataProvider d)
		{
			Set(d.Id, d, TimeSpan.Zero);
		}

		public IDataProvider Select(Guid id)
		{
			return Get(id);
		}
	}
}
