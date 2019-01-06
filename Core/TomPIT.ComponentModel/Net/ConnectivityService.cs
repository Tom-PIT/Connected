using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Exceptions;

namespace TomPIT.Net
{
	internal class ConnectivityService : StatefulCacheRepository<ISysContext, string>, IConnectivityService
	{
		private Lazy<List<ISysConnectionDescriptor>> _connections = new Lazy<List<ISysConnectionDescriptor>>();
		public event ContextRegisteredHandler ContextRegistered;
		public event ContextRegisteredHandler ContextInitializing;

		public ConnectivityService() : base(MemoryCache.Default, "syscontext")
		{

		}


		public ISysContext Select()
		{
			return First();
		}

		public ISysContext Select(string url)
		{
			return Get(url);
		}

		public void Insert(string name, string url, string clientKey)
		{
			if (Get(url) != null)
				throw new ServerException(SR.ErrSysContextRegistered);

			var instance = new ServerContext(url, clientKey);

			Connections.Add(new SysConnectionDescriptor(name, url, clientKey));

			Set(url, instance, TimeSpan.Zero);
			ContextRegistered?.Invoke(this, new SysContextRegisteredArgs(instance));
			ContextInitializing?.Invoke(this, new SysContextRegisteredArgs(instance));
		}

		public List<ISysConnectionDescriptor> QueryConnections()
		{
			return Connections;
		}

		private List<ISysConnectionDescriptor> Connections { get { return _connections.Value; } }
	}
}
