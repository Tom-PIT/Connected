using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Exceptions;

namespace TomPIT.Connectivity
{
	internal class ConnectivityService : SynchronizedRepository<ITenant, string>, IConnectivityService
	{
		private Lazy<List<ITenantDescriptor>> _connections = new Lazy<List<ITenantDescriptor>>();
		public event TenantHandler TenantInitialized;
		public event TenantHandler TenantInitialize;
		public event TenantHandler TenantInitializing;

		public ConnectivityService() : base(MemoryCache.Default, "syscontext")
		{

		}


		public ITenant SelectDefaultTenant()
		{
			return First();
		}

		public ITenant SelectTenant(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return SelectDefaultTenant();

			return Get(url);
		}

		public void InsertTenant(string name, string url, string authenticationToken)
		{
			if (Get(url) != null)
				throw new RuntimeException(SR.ErrSysContextRegistered);

			var instance = new Tenant(url, authenticationToken);

			Connections.Add(new TenantDescriptor(name, url, authenticationToken));

			Set(url, instance, TimeSpan.Zero);
			TenantInitializing?.Invoke(this, new TenantArgs(instance));
			TenantInitialize?.Invoke(this, new TenantArgs(instance));
			TenantInitialized?.Invoke(this, new TenantArgs(instance));
		}

		public List<ITenantDescriptor> QueryTenants()
		{
			return Connections;
		}

		private List<ITenantDescriptor> Connections { get { return _connections.Value; } }
	}
}
