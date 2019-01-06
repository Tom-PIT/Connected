using System;
using System.Collections.Generic;
using TomPIT.Net;

namespace TomPIT.ComponentModel.DataProviders
{
	internal class DataProviderService : ContextCacheRepository<IDataProvider, Guid>, IDataProviderService
	{
		public DataProviderService(ISysContext server) : base(server, "dataprovider")
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
