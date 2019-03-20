using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Data
{
	internal class Messages : SynchronizedRepository<IMessage, Guid>
	{
		public Messages(IMemoryCache container) : base(container, "message")
		{
		}

		protected override void OnInitializing()
		{
			var rgs = DataModel.ResourceGroups.Query();

			Parallel.ForEach(rgs,
				(i) =>
				{
					var ds = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.QueryMessages(i);

					foreach (var j in ds)
						Set(j.Token, j, TimeSpan.Zero);
				});
		}

		protected override void OnInvalidate(Guid id)
		{
			var rgs = DataModel.ResourceGroups.Query();

			foreach (var i in rgs)
			{
				var r = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.SelectMessage(i, id);

				if (r != null)
				{
					Set(id, r, TimeSpan.Zero);
					return;
				}
			}

			Remove(id);
		}

		public IMessage Select(Guid id)
		{
			return Get(id,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					var rgs = DataModel.ResourceGroups.Query();

					foreach (var i in rgs)
					{
						var r = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.SelectMessage(i, id);

						if (r != null)
							return r;
					}

					return null;
				});
		}

		public List<IMessage> Query()
		{
			return All();
		}

		public List<IMessage> Query(string topic)
		{
			return Where(f => string.Compare(f.Topic, topic, true) == 0);
		}

		public void Insert(string topic, Guid token, string content, DateTime expire, TimeSpan retryInterval, Guid senderInstance)
		{
			var t = DataModel.MessageTopics.Ensure(topic);

			if (t == null)
				throw new SysException(string.Format("{0} ({1})", SR.ErrTopicNotFound, topic));

			var rg = DataModel.ResourceGroups.Select(t.ResourceGroup);

			if (rg == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.InsertMessage(rg, t, token, content, expire, retryInterval, senderInstance);

			Refresh(token);
			DataModel.MessageRecipients.Load(token);
		}

		public void Delete(Guid message)
		{
			var m = Select(message);

			if (m == null)
				return;

			DataModel.MessageRecipients.Delete(message);
			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.DeleteMessage(m.ResolveResourceGroup(), m);

			Remove(message);
		}
	}
}
