using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Diagostics;
using TomPIT.Exceptions;
using TomPIT.Storage;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

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

		public byte[] Download([CAP(CAP.MicroservicesProvider)]string microService, string primaryKey)
		{
			var ms = Context.Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({microService})").WithMetrics(Context);

			return Context.Tenant.GetService<IStorageService>().Download(ms.Token, BlobTypes.UserContent, ms.ResourceGroup, primaryKey)?.Content;
		}

		public void Delete([CAP(CAP.MicroservicesProvider)]string microService, string primaryKey)
		{
			var ms = Context.Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({microService})").WithMetrics(Context);

			var blobs = Context.Tenant.GetService<IStorageService>().Query(ms.Token, BlobTypes.UserContent, ms.ResourceGroup, primaryKey);

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
				Context.Tenant.LogWarning(Context, nameof(MiddlewareStorageService), ex.Message);
			}
		}

		public List<IBlob> Query([CAP(CAP.MicroservicesProvider)]string microService, string primaryKey)
		{
			var ms = Context.Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({microService})").WithMetrics(Context);

			return Context.Tenant.GetService<IStorageService>().Query(ms.Token, BlobTypes.UserContent, ms.ResourceGroup, primaryKey);
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
			if (!(Context is IMicroServiceContext msc))
				throw new RuntimeException(SR.ErrMicroServiceContextExpected);

			var ms = msc.MicroService.Token;
			Guid rg = Guid.Empty;

			if (ms != Guid.Empty)
			{
				var m = Context.Tenant.GetService<IMicroServiceService>().Select(ms);

				if (m != null)
					rg = m.ResourceGroup;
			}

			var b = new Blob
			{
				ContentType = contentType,
				Draft = draft,
				FileName = fileName,
				MicroService = ms,
				ResourceGroup = rg,
				PrimaryKey = primaryKey,
				Topic = topic,
				Type = BlobTypes.UserContent
			};

			return Context.Tenant.GetService<IStorageService>().Upload(b, content, policy, token);
		}
	}
}