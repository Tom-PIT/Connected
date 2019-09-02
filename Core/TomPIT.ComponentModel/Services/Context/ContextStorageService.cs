using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Storage;

namespace TomPIT.Services.Context
{
	internal class ContextStorageService : ContextClient, IContextStorageService
	{
		public ContextStorageService(IExecutionContext context) : base(context)
		{
		}

		public void CommitDrafts(string draft, string primaryKey)
		{
			Context.Connection().GetService<IStorageService>().Commit(draft, primaryKey);
		}

		public byte[] Download(Guid blob)
		{
			return Context.Connection().GetService<IStorageService>().Download(blob)?.Content;
		}

        public void Delete(Guid blob)
        {
            try
            {
                Context.Connection().GetService<IStorageService>().Delete(blob);
            }
            catch(Exception ex)
            {
                Context.Connection().LogWarning(Context, nameof(ContextStorageService), ex.Message);
            }
        }

		public List<IBlob> QueryDrafts(string draft)
		{
			return Context.Connection().GetService<IStorageService>().QueryDrafts(draft);
		}

		public Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic)
		{
			return Upload(policy, fileName, contentType, primaryKey, content, topic, string.Empty);
		}

		public Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic, string draft)
		{
			var ms = Context.MicroService.Token;
			Guid rg = Guid.Empty;

			if (ms != Guid.Empty)
			{
				var m = Context.Connection().GetService<IMicroServiceService>().Select(ms);

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
				Type = BlobTypes.UserContent,
			};

			return Context.Connection().GetService<IStorageService>().Upload(b, content, policy);
		}
	}
}
