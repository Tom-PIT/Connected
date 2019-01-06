using System;
using System.Collections.Generic;
using TomPIT.Api.Storage;
using TomPIT.Caching;
using TomPIT.Storage;

namespace TomPIT.Sys.Data
{
	internal class BlobsContents : CacheRepository<IBlobContent, string>
	{
		public BlobsContents(IMemoryCache container) : base(container, "blobcontent")
		{

		}

		public List<IBlobContent> Query(List<Guid> blobs)
		{
			var ds = DataModel.Blobs.Query(blobs);
			var r = new List<IBlobContent>();

			foreach (var i in ds)
			{
				var content = Select(i);

				if (content != null)
					r.Add(content);
			}

			return r;
		}

		public IBlobContent Select(Guid blob)
		{
			return Select(DataModel.Blobs.Select(blob));
		}

		private IBlobContent Select(IBlob blob)
		{
			if (blob == null)
				throw new SysException(SR.ErrBlobNotFound);

			return Get(GenerateKey(blob.ResourceGroup, blob.Token),
				(f) =>
				{
					var rg = DataModel.ResourceGroups.Select(blob.ResourceGroup);

					if (rg == null)
						throw new SysException(string.Format("{0} ({1})", SR.ErrResourceGroupNotFound, blob.ResourceGroup));

					var sp = Shell.GetService<IStorageProviderService>().Resolve(blob.ResourceGroup);

					return sp.Blobs.Download(rg, blob.Token);
				});
		}

		public void Update(Guid resourceGroup, Guid token, byte[] content)
		{
			var rg = resourceGroup == Guid.Empty
				? DataModel.ResourceGroups.Default
				: DataModel.ResourceGroups.Select(resourceGroup);
			
			if (rg == null)
				throw new SysException(string.Format("{0} ({1})", SR.ErrResourceGroupNotFound, resourceGroup));

			var sp = Shell.GetService<IStorageProviderService>().Resolve(resourceGroup);

			sp.Blobs.Upload(rg, token, content);

			Remove(GenerateKey(resourceGroup, token));
		}

		public void Delete(Guid resourceGroup, Guid token)
		{
			var rg = resourceGroup == Guid.Empty
				? DataModel.ResourceGroups.Default
				: DataModel.ResourceGroups.Select(resourceGroup);

			if (rg == null)
				throw new SysException(string.Format("{0} ({1})", SR.ErrResourceGroupNotFound, resourceGroup));

			var sp = Shell.GetService<IStorageProviderService>().Resolve(resourceGroup);

			sp.Blobs.Delete(rg, token);

			Remove(GenerateKey(resourceGroup, token));
		}
	}
}