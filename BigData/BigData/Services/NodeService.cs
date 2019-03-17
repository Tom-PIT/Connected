using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.BigData.Services
{
	internal class NodeService : SynchronizedClientRepository<INode, Guid>, INodeService
	{
		protected NodeService(ISysConnection connection) : base(connection, "bigdatanode")
		{
		}

		public INode Select(Guid token)
		{
			return Get(token);
		}

		public INode SelectSmallest()
		{
			if (Count == 0)
				return null;

			return All().OrderBy(f => f.Size).First();
		}

		protected override void OnInitializing()
		{
			var u = Connection.CreateUrl("BigDataManagement", "QueryNodes");
			var ds = Connection.Get<List<Node>>(u);

			foreach (var node in ds)
				Set(node.Token, node, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Connection.CreateUrl("BigDataManagement", "SelectNode");
			var e = new JObject
			{
				{"token", id }
			};

			var node = Connection.Post<Node>(u, e);

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
