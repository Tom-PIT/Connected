using System;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	public abstract class ConnectionMiddleware : MiddlewareComponent, IConnectionMiddleware
	{
		public IConnectionString Invoke(ConnectionMiddlewareArgs e)
		{
			ConnectionContext = e.ConnectionContext;

			Validate();
			return OnInvoke();
		}

		protected virtual IConnectionString OnInvoke()
		{
			return null;
		}

		protected ImmutableList<IDataProvider> DataProviders => Context.Tenant.GetService<IDataProviderService>().Query();

		public ConnectionStringContext ConnectionContext { get; private set; } = ConnectionStringContext.User;

		protected Guid ResolveProvider(string name)
		{
			var provider = DataProviders.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0);

			if (provider == null)
				return Guid.Empty;

			return provider.Id;
		}
	}
}
