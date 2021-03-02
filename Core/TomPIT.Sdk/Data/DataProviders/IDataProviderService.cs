using System;
using System.Collections.Immutable;

namespace TomPIT.Data.DataProviders

{
	public interface IDataProviderService
	{
		ImmutableList<IDataProvider> Query();
		void Register(IDataProvider d);
		IDataProvider Select(Guid id);
	}
}