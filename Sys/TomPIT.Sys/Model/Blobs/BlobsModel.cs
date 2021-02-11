using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Blobs
{
	public class BlobsModel : CacheRepository<IBlob, Guid>
	{
		public const int Avatar = 11;

		public BlobsModel(IMemoryCache container) : base(container, "blob")
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

		public void Commit(string draft, string primaryKey)
		{
			var cached = Where(f => string.Compare(f.Draft, draft, true) == 0);

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
			var r = resourceGroup == Guid.Empty
				? DataModel.ResourceGroups.Default
				: DataModel.ResourceGroups.Select(resourceGroup);

			if (r == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Storage.Query(r, type, primaryKey);
		}

		public List<IBlob> Query(Guid resourceGroup, int type, string primaryKey, Guid microService, string topic)
		{
			var r = resourceGroup == Guid.Empty
				? DataModel.ResourceGroups.Default
				: DataModel.ResourceGroups.Select(resourceGroup);

			if (r == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			if (microService != Guid.Empty && SupportsNotification(type))
			{
				var s = DataModel.MicroServices.Select(microService);

				if (s == null)
					throw new SysException(SR.ErrMicroServiceNotFound);
			}

			return Shell.GetService<IDatabaseService>().Proxy.Storage.Query(r, type, primaryKey, microService, topic);
		}

		public List<IBlob> Query(Guid microService)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Storage.Query(microService);
		}

		public List<IBlob> QueryDrafts(string draft)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Storage.QueryDrafts(draft);
		}

		public void Deploy(Guid resourceGroup, int type, string primaryKey, Guid microService, string topic, string fileName, string contentType, byte[] content,
			StoragePolicy policy, Guid token, int version)
		{
			var r = resourceGroup == Guid.Empty
				? DataModel.ResourceGroups.Default
				: DataModel.ResourceGroups.Select(resourceGroup);

			if (r == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			if (microService != Guid.Empty && SupportsNotification(type))
			{
				var s = DataModel.MicroServices.Select(microService);

				if (s == null)
					throw new SysException(SR.ErrMicroServiceNotFound);
			}

			var existing = Select(token);

			if (existing != null)
			{
				Shell.GetService<IDatabaseService>().Proxy.Storage.Update(existing, primaryKey, fileName,
					contentType, content == null ? 0 : content.Length, version, DateTime.UtcNow, string.Empty);

				DataModel.BlobsContents.Update(resourceGroup, token, content);

				if (SupportsNotification(type))
					CachingNotifications.BlobChanged(microService, token, type, primaryKey);
			}
			else
			{
				Shell.GetService<IDatabaseService>().Proxy.Storage.Insert(r, token, type, primaryKey, microService, topic, fileName,
					contentType, content == null ? 0 : content.Length, version, DateTime.UtcNow, string.Empty);

				DataModel.BlobsContents.Update(resourceGroup, token, content);

				if (SupportsNotification(type))
					CachingNotifications.BlobAdded(microService, token, type, primaryKey);
			}
		}

		public void Clean(Guid microService)
		{
			var blobs = Shell.GetService<IDatabaseService>().Proxy.Storage.QueryByLevel(microService, 500);

			foreach (var i in blobs)
				Delete(i.Token);
		}

		public Guid Upload(Guid resourceGroup, int type, string primaryKey, Guid microService, string topic, string fileName, string contentType, string draft, byte[] content,
			StoragePolicy policy, Guid token)
		{
			if (!string.IsNullOrEmpty(draft) || policy == StoragePolicy.Extended)
			{
				var existingDrafts = QueryDrafts(draft);
				var existingDraft = existingDrafts.FirstOrDefault(f => string.Compare(f.FileName, fileName, true) == 0);

				if (existingDraft == null)
					return Insert(token, resourceGroup, type, primaryKey, microService, topic, fileName, contentType, draft, content);
				else
				{
					Update(existingDraft.Token, null, fileName, contentType, draft, content);

					return existingDraft.Token;
				}
			}

			var r = resourceGroup == Guid.Empty
				? DataModel.ResourceGroups.Default
				: DataModel.ResourceGroups.Select(resourceGroup);

			if (r == null)
				throw new SysException(SR.ErrResourceGroupNotFound);
			/*
			 * we only check for the microservice existence in the case of tighly bound blobs
			 * to the microservices. installer configuration, database state and similar does
			 * not require a microservice to be actually installed
			 */
			if (microService != Guid.Empty && SupportsNotification(type))
			{
				var s = DataModel.MicroServices.Select(microService);

				if (s == null)
					throw new SysException(SR.ErrMicroServiceNotFound);
			}

			var ds = Shell.GetService<IDatabaseService>().Proxy.Storage.Query(r, type, primaryKey, microService, topic);

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

		public Guid Insert(Guid token, Guid resourceGroup, int type, string primaryKey, Guid microService, string topic, string fileName, string contentType, string draft, byte[] content)
		{
			var r = resourceGroup == Guid.Empty
				? DataModel.ResourceGroups.Default
				: DataModel.ResourceGroups.Select(resourceGroup);

			if (r == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			if (microService != Guid.Empty && SupportsNotification(type))
			{
				var s = DataModel.MicroServices.Select(microService);

				if (s == null)
					throw new SysException(SR.ErrMicroServiceNotFound);
			}

			if (token == Guid.Empty)
				token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Storage.Insert(r, token, type, primaryKey, microService, topic, fileName, contentType, content == null ? 0 : content.Length, 1, DateTime.UtcNow, draft);
			DataModel.BlobsContents.Update(resourceGroup, token, content);

			if (SupportsNotification(type))
				CachingNotifications.BlobAdded(microService, token, type, primaryKey);

			return token;
		}

		public void Update(Guid token, string primaryKey, string fileName, string contentType, string draft, byte[] content)
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
