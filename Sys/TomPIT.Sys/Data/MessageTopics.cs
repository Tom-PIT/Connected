using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Api.Net;
using TomPIT.Api.Storage;
using TomPIT.Caching;

namespace TomPIT.Sys.Data
{
	internal class MessageTopics : SynchronizedRepository<ITopic, string>
	{
		public MessageTopics(IMemoryCache container) : base(container, "messagetopic")
		{
		}

		protected override void OnInitializing()
		{
			var rgs = DataModel.ResourceGroups.Query();

			Parallel.ForEach(rgs,
				(i) =>
				{
					var ds = Shell.GetService<IStorageProviderService>().Resolve(i.Token).Messaging.QueryTopics(i);

					foreach (var j in ds)
						Set(j.Name, j, TimeSpan.Zero);

				});
		}

		protected override void OnInvalidate(string id)
		{
			var rgs = DataModel.ResourceGroups.Query();

			foreach (var i in rgs)
			{
				var r = Shell.GetService<IStorageProviderService>().Resolve(Guid.Empty).Messaging.SelectTopic(i, id);

				if (r != null)
				{
					Set(id, r, TimeSpan.Zero);
					return;
				}
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

					var rgs = DataModel.ResourceGroups.Query();

					foreach (var i in rgs)
					{
						var r = Shell.GetService<IStorageProviderService>().Resolve(Guid.Empty).Messaging.SelectTopic(i, id);

						if (r != null)
							return r;
					}

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

			Shell.GetService<IStorageProviderService>().Resolve(rg.Token).Messaging.InsertTopic(rg, name);

			Refresh(name);
		}

		public void Delete(string name)
		{
			var t = Select(name);

			if (t == null)
				throw new SysException(SR.ErrTopicNotFound);

			var rg = t.ResolveResourceGroup();

			Shell.GetService<IStorageProviderService>().Resolve(rg.Token).Messaging.DeleteTopic(rg, t);

			Remove(name);
		}
	}
}
