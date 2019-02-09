using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
	public class Blobs : CacheRepository<IBlob, Guid>
	{
		public const int Avatar = 11;

		public Blobs(IMemoryCache container) : base(container, "blob")
		{
		}

		public IBlob Select(Guid token)
		{
			return Get(token,
				(f) =>
				{
					return Shell.GetService<IDatabaseService>().Proxy.Storage.Select(token);
				});
		}

		public void Commit(Guid draft, string primaryKey)
		{
			var cached = Where(f => f.Draft == draft);

			Shell.GetService<IDatabaseService>().Proxy.Storage.Commit(draft, primaryKey);

			foreach (var i in cached)
			{
				Remove(i.Token);

				if (SupportsNotification(i.Type))
					CachingNotifications.BlobCommitted(i.MicroService, i.Token, i.Type, i.PrimaryKey);
			}
		}

		public void Delete(Guid blob)
		{
			var b = Select(blob);

			if (b == null)
				throw new SysException(SR.ErrBlobNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Storage.Delete(b);

			Remove(blob);

			DataModel.BlobsContents.Delete(b.ResourceGroup, blob);

			if (SupportsNotification(b.Type))
				CachingNotifications.BlobRemoved(b.MicroService, b.Token, b.Type, b.PrimaryKey);
		}

		public List<IBlob> Query(List<Guid> blobs)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Storage.Query(blobs);
		}

		public List<IBlob> Query(Guid resourceGroup, int type, string primaryKey)
		{
			var r = DataModel.ResourceGroups.Select(resourceGroup);

			if (r == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Storage.Query(r, type, primaryKey);
		}

		public List<IBlob> Query(Guid resourceGroup, int type, string primaryKey, Guid microService, string topic)
		{
			var r = DataModel.ResourceGroups.Select(resourceGroup);

			if (r == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var s = DataModel.MicroServices.Select(microService);

			if (s == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Storage.Query(r, type, primaryKey, s, topic);
		}

		public List<IBlob> Query(Guid microService)
		{
			var s = DataModel.MicroServices.Select(microService);

			if (s == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Storage.Query(s);
		}

		public List<IBlob> QueryDrafts(Guid draft)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Storage.QueryDrafts(draft);
		}

		public Guid Upload(Guid resourceGroup, int type, string primaryKey, Guid microService, string topic, string fileName, string contentType, Guid draft, byte[] content, StoragePolicy policy, Guid token)
		{
			if (draft != Guid.Empty || policy == StoragePolicy.Extended)
				return Insert(token, resourceGroup, type, primaryKey, microService, topic, fileName, contentType, draft, content);

			var r = resourceGroup == Guid.Empty
				? DataModel.ResourceGroups.Default
				: DataModel.ResourceGroups.Select(resourceGroup);

			if (r == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			IMicroService s = null;

			if (microService != Guid.Empty)
			{
				s = DataModel.MicroServices.Select(microService);

				if (s == null)
					throw new SysException(SR.ErrMicroServiceNotFound);
			}

			var ds = Shell.GetService<IDatabaseService>().Proxy.Storage.Query(r, type, primaryKey, s, topic);

			if (policy == StoragePolicy.Singleton)
			{
				if (ds.Count == 0)
					return Insert(token, resourceGroup, type, primaryKey, microService, topic, fileName, contentType, draft, content);
				else
				{
					Update(ds[0].Token, primaryKey, fileName, contentType, draft, content);

					return ds[0].Token;
				}
			}
			else
			{
				var existing = ds.FirstOrDefault(f => string.Compare(f.FileName, fileName, true) == 0);

				if (existing == null)
					return Insert(token, resourceGroup, type, primaryKey, microService, topic, fileName, contentType, draft, content);
				else
				{
					Update(existing.Token, primaryKey, fileName, contentType, draft, content);

					return existing.Token;
				}

			}
		}

		public Guid Insert(Guid token, Guid resourceGroup, int type, string primaryKey, Guid microService, string topic, string fileName, string contentType, Guid draft, byte[] content)
		{
			var r = resourceGroup == Guid.Empty
				? DataModel.ResourceGroups.Default
				: DataModel.ResourceGroups.Select(resourceGroup);

			if (r == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			IMicroService s = null;

			if (microService != Guid.Empty)
			{
				s = DataModel.MicroServices.Select(microService);

				if (s == null)
					throw new SysException(SR.ErrMicroServiceNotFound);
			}

			if (token == Guid.Empty)
				token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Storage.Insert(r, token, type, primaryKey, s, topic, fileName, contentType, content == null ? 0 : content.Length, 1, DateTime.UtcNow, draft);
			DataModel.BlobsContents.Update(resourceGroup, token, content);

			if (SupportsNotification(type))
				CachingNotifications.BlobAdded(microService, token, type, primaryKey);

			return token;
		}

		public void Update(Guid token, string primaryKey, string fileName, string contentType, Guid draft, byte[] content)
		{
			var blob = Select(token);

			if (blob == null)
				throw new SysException(SR.ErrBlobNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Storage.Update(blob, primaryKey, fileName, contentType, content == null ? 0 : content.Length, blob.Version + 1, DateTime.UtcNow, draft);
			DataModel.BlobsContents.Update(blob.ResourceGroup, token, content);

			Refresh(blob.Token);

			if (SupportsNotification(blob.Type))
				CachingNotifications.BlobChanged(blob.MicroService, blob.Token, blob.Type, blob.PrimaryKey);
		}

		private bool SupportsNotification(int type)
		{
			return type < 1000;
		}
	}
}
