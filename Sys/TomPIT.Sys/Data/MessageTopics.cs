using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Data
{
	internal class MessageTopics : SynchronizedRepository<ITopic, string>
	{
		public MessageTopics(IMemoryCache container) : base(container, "messagetopic")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.QueryTopics();

			foreach (var j in ds)
				Set(j.Name, j, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.SelectTopic(id);

			if (r != null)
			{
				Set(id, r, TimeSpan.Zero);
				return;
			}

			Remove(id);
		}

		public ITopic Ensure(string name)
		{
			var r = Select(name);

			if (r != null)
				return r;

			Insert(DataModel.ResourceGroups.Default.Token, name);

			return Select(name);
		}

		public ITopic Select(string id)
		{
			return Get(id,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					var r = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.SelectTopic(id);

					if (r != null)
						return r;

					return null;
				});
		}

		public List<ITopic> Query()
		{
			return All();
		}

		public void Insert(Guid resourceGroup, string name)
		{
			var rg = DataModel.ResourceGroups.Select(resourceGroup);

			if (rg == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.InsertTopic(rg, name);

			Refresh(name);
		}

		public void Delete(string name)
		{
			var t = Select(name);

			if (t == null)
				throw new SysException(SR.ErrTopicNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.DeleteTopic(t);

			Remove(name);
		}
	}
}
