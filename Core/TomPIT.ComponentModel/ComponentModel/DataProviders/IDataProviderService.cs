using System;
using System.Collections.Generic;

namespace TomPIT.ComponentModel.DataProviders

{
	public interface IDataProviderService
	{
		List<IDataProvider> Query();
		void Register(IDataProvider d);
		IDataProvider Select(Guid id);
	}
}