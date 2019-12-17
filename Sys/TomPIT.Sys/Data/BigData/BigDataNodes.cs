using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data.BigData
{
	internal class BigDataNodes : SynchronizedRepository<INode, Guid>
	{
		public BigDataNodes(IMemoryCache container) : base(container, "bigdatanodes")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.BigData.Nodes.Query();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.BigData.Nodes.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public INode Select(Guid token)
		{
			return Get(token);
		}

		public List<INode> Query()
		{
			return All();
		}

		public Guid Insert(string name, string connectionString, string adminConnectionString, NodeStatus status)
		{
			var id = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.BigData.Nodes.Insert(id, name, connectionString, adminConnectionString, status);

			Refresh(id);
			BigDataNotifications.NodeAdded(id);

			return id;
		}

		public void Update(Guid token, string name, string connectionString, string adminConnectionString, NodeStatus status, long size)
		{
			var node = Select(token);

			if (node == null)
				throw new SysException(SR.ErrBigDataNodeNotFound);

			Shell.GetService<IDatabaseService>().Proxy.BigData.Nodes.Update(node, name, connectionString, adminConnectionString, status, size);

			Refresh(node.Token);
			BigDataNotifications.NodeChanged(node.Token);
		}

		public void Delete(Guid token)
		{
			var node = Select(token);

			if (node == null)
				throw new SysException(SR.ErrBigDataNodeNotFound);

			Shell.GetService<IDatabaseService>().Proxy.BigData.Nodes.Delete(node);

			Refresh(node.Token);
			BigDataNotifications.NodeRemoved(node.Token);
		}
	}
}
