using System;
using System.Collections.Generic;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Storage;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareStorageService : MiddlewareObject, IMiddlewareStorageService
	{
		public MiddlewareStorageService(IMiddlewareContext context) : base(context)
		{
		}

		public void CommitDrafts(string draft, string primaryKey)
		{
			Context.Tenant.GetService<IStorageService>().Commit(draft, primaryKey);
		}

		public byte[] Download(Guid blob)
		{
			return Context.Tenant.GetService<IStorageService>().Download(blob)?.Content;
		}

		public byte[] Download(string primaryKey)
		{
			return Download(primaryKey, null);
		}

		public byte[] Download(string primaryKey, string topic)
		{
			return Context.Tenant.GetService<IStorageService>().Download(Guid.Empty, BlobTypes.UserContent, Guid.Empty, primaryKey, topic)?.Content;
		}

		public void Delete(string primaryKey)
		{
			Delete(primaryKey, null);
		}

		public void Delete(string primaryKey, string topic)
		{
			var blobs = Context.Tenant.GetService<IStorageService>().Query(Guid.Empty, BlobTypes.UserContent, Guid.Empty, primaryKey, topic);

			if (blobs != null && blobs.Count > 0)
				Context.Tenant.GetService<IStorageService>().Delete(blobs[0].Token);
		}

		public void Delete(Guid blob)
		{
			try
			{
				Context.Tenant.GetService<IStorageService>().Delete(blob);
			}
			catch (Exception ex)
			{
				Context.Tenant.LogWarning(ex.Message, LogCategories.Middleware, nameof(MiddlewareStorageService));
			}
		}

		public List<IBlob> Query(string primaryKey)
		{
			return Query(primaryKey, null);
		}
		public List<IBlob> Query(string primaryKey, string topic)
		{
			return Context.Tenant.GetService<IStorageService>().Query(Guid.Empty, BlobTypes.UserContent, Guid.Empty, primaryKey, topic);
		}

		public List<IBlob> QueryDrafts(string draft)
		{
			return Context.Tenant.GetService<IStorageService>().QueryDrafts(draft);
		}

		public Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic, Guid token)
		{
			return Upload(policy, fileName, contentType, primaryKey, content, topic, string.Empty, token);
		}

		public Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic)
		{
			return Upload(policy, fileName, contentType, primaryKey, content, topic, string.Empty);
		}

		public Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic, string draft)
		{
			return Upload(policy, fileName, contentType, primaryKey, content, topic, draft, Guid.Empty);
		}
		private Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic, string draft, Guid token)
		{
			var b = new Blob
			{
				ContentType = contentType,
				Draft = draft,
				FileName = fileName,
				PrimaryKey = primaryKey,
				Topic = topic,
				Type = BlobTypes.UserContent
			};

			return Context.Tenant.GetService<IStorageService>().Upload(b, content, policy, token);
		}
	}
}