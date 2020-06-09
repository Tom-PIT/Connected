using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	public abstract class ConnectionMiddleware : MiddlewareComponent, IConnectionMiddleware
	{
		public IConnectionString Invoke()
		{
			Validate();
			return OnInvoke();
		}

		protected virtual IConnectionString OnInvoke()
		{
			return null;
		}

		protected List<IDataProvider> DataProviders => Context.Tenant.GetService<IDataProviderService>().Query();

		protected Guid ResolveProvider(string name)
		{
			var provider = DataProviders.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0);

			if (provider == null)
				return Guid.Empty;

			return provider.Id;
		}
	}
}
