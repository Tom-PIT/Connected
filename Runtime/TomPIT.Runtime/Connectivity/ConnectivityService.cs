using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Exceptions;

namespace TomPIT.Connectivity
{
	internal class ConnectivityService : SynchronizedRepository<ISysConnection, string>, IConnectivityService
	{
		private Lazy<List<ISysConnectionDescriptor>> _connections = new Lazy<List<ISysConnectionDescriptor>>();
		public event ConnectionRegisteredHandler ConnectionRegistered;
		public event ConnectionRegisteredHandler ConnectionInitializing;

		public ConnectivityService() : base(MemoryCache.Default, "syscontext")
		{

		}


		public ISysConnection Select()
		{
			return First();
		}

		public ISysConnection Select(string url)
		{
			return Get(url);
		}

		public void Insert(string name, string url, string clientKey)
		{
			if (Get(url) != null)
				throw new ExecutionException(SR.ErrSysContextRegistered);

			var instance = new SysConnection(url, clientKey);

			Connections.Add(new SysConnectionDescriptor(name, url, clientKey));

			Set(url, instance, TimeSpan.Zero);
			ConnectionRegistered?.Invoke(this, new SysConnectionRegisteredArgs(instance));
			ConnectionInitializing?.Invoke(this, new SysConnectionRegisteredArgs(instance));
		}

		public List<ISysConnectionDescriptor> QueryConnections()
		{
			return Connections;
		}

		private List<ISysConnectionDescriptor> Connections { get { return _connections.Value; } }
	}
}
