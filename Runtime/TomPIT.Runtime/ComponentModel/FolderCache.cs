using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	internal class FolderCache : SynchronizedClientRepository<IFolder, Guid>
	{
		public FolderCache(ISysConnection connection) : base(connection, "folder")
		{
		}

		protected override void OnInitializing()
		{
			var u = Connection.CreateUrl("Folder", "Query");
			var ds = Connection.Get<List<Folder>>(u);

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Connection.CreateUrl("Folder", "SelectByToken")
				.AddParameter("token", id);

			var d = Connection.Get<Folder>(u);

			if (d == null)
				return;

			Set(d.Token, d, TimeSpan.Zero);
		}

		public IFolder Select(Guid token)
		{
			return Get(token);
		}

		public List<IFolder> Query(Guid microService, Guid parent)
		{
			return Where(f => f.MicroService == microService && f.Parent == parent);
		}

		public void Reload(Guid id)
		{
			Refresh(id);
		}

		public void Delete(Guid id)
		{
			Remove(id);
		}
	}
}
