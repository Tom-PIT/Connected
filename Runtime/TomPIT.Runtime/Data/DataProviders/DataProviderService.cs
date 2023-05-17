using System;
using System.Collections.Immutable;
using System.Text.Json;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Reflection;

namespace TomPIT.Data.DataProviders
{
	internal class DataProviderService : ClientRepository<IDataProvider, Guid>, IDataProviderService
	{
		public DataProviderService(ITenant tenant) : base(tenant, "dataprovider")
		{
			Initialize();
		}

		public ImmutableList<IDataProvider> Query()
		{
			return All();
		}

		public void Register(IDataProvider provider)
		{
			Set(provider.Id, provider, TimeSpan.Zero);
		}

		public IDataProvider Select(Guid id)
		{
			return Get(id);
		}

		private void Initialize()
		{
			if (!Shell.Configuration.RootElement.TryGetProperty("dataProviders", out JsonElement element))
				return;

			foreach (var item in element.EnumerateArray())
			{
				var t = TypeExtensions.GetType(item.GetString());

				if (t is null)
					continue;

				var provider = t.CreateInstance<IDataProvider>();

				if (provider is not null)
					Register(provider);
			}
		}
	}
}
