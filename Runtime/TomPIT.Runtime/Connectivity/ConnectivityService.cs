using System;
using System.Collections.Generic;
using TomPIT.Caching;

namespace TomPIT.Connectivity
{
	internal class ConnectivityService : SynchronizedRepository<ISysConnection, string>, IConnectivityService
	{
		private Lazy<List<ISysConnectionDescriptor>> _connections = new Lazy<List<ISysConnectionDescriptor>>();
		public event ConnectionHandler ConnectionInitialized;
		public event ConnectionHandler ConnectionInitialize;
		public event ConnectionHandler ConnectionInitializing;

		public ConnectivityService() : base(MemoryCache.Default, "syscontext")
		{

		}


		public ISysConnection Select()
		{
			return First();
		}

		public ISysConnection Select(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return Select();

			return Get(url);
		}

		public void Insert(string name, string url, string authenticationToken)
		{
			if (Get(url) != null)
				throw new RuntimeException(SR.ErrSysContextRegistered);

			var instance = new SysConnection(url, authenticationToken);

			Connections.Add(new SysConnectionDescriptor(name, url, authenticationToken));

			Set(url, instance, TimeSpan.Zero);
			ConnectionInitializing?.Invoke(this, new SysConnectionArgs(instance));
			ConnectionInitialize?.Invoke(this, new SysConnectionArgs(instance));
			ConnectionInitialized?.Invoke(this, new SysConnectionArgs(instance));
		}

		public List<ISysConnectionDescriptor> QueryConnections()
		{
			return Connections;
		}

		private List<ISysConnectionDescriptor> Connections { get { return _connections.Value; } }
	}
}
