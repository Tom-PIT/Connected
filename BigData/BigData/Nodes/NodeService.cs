using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

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
			var u = Tenant.CreateUrl("BigDataManagement", "QueryNodes");
			var ds = Tenant.Get<List<Node>>(u);

			foreach (var node in ds)
				Set(node.Token, node, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "SelectNode");
			var e = new JObject
			{
				{"token", id }
			};

			var node = Tenant.Post<Node>(u, e);

			if (node == null)
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
