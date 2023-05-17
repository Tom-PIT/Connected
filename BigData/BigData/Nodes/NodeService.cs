using System;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.BigData.Nodes
{
	internal class NodeService : SynchronizedClientRepository<INode, Guid>, INodeService
	{
		public NodeService(ITenant tenant) : base(tenant, "bigdatanode")
		{
		}

		public INode Select(Guid token)
		{
			return Get(token);
		}

		public INode SelectSmallest()
		{
			Initialize();

			if (Count == 0)
				return null;

			return All().OrderBy(f => f.Size).First();
		}

		protected override void OnInitializing()
		{
			var ds = Instance.SysProxy.Management.BigData.QueryNodes();

			foreach (var node in ds)
				Set(node.Token, node, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var node = Instance.SysProxy.Management.BigData.SelectNode(id);

			if (node is null)
				return;

			Set(node.Token, node, TimeSpan.Zero);
		}

		public void NotifyChanged(Guid token)
		{
			Refresh(token);
		}

		public void NotifyRemoved(Guid token)
		{
			Remove(token);
		}
	}
}
