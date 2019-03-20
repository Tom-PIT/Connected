using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TomPIT.Api.Storage;
using TomPIT.Storage;
using TomPIT.SysDb.Environment;

namespace TomPIT.StorageProvider.Azure
{
	internal class Blobs : IBlobProvider
	{
		public void Delete(IServerResourceGroup resourceGroup, Guid blob)
		{
			var ablob = AzureUtils.GetBlobReference(AzureUtils.StorageContainer, blob.AsString());

			Task.Run(async () =>
			{
				if (!ablob.ExistsAsync().Result)
					return;

				await ablob.DeleteAsync();
			});
		}

		public IBlobContent Download(IServerResourceGroup resourceGroup, Guid blob)
		{
			var ablob = AzureUtils.GetBlobReference(AzureUtils.StorageContainer, blob.AsString());

			var result = Task.Run<IBlobContent>(async () =>
			 {
				 if (!ablob.ExistsAsync().Result)
					 return null;

				 var r = new BlobContent
				 {
					 Blob = blob
				 };

				 using (var ms = new MemoryStream())
				 {
					 await ablob.DownloadToStreamAsync(ms);

					 ms.Seek(0, SeekOrigin.Begin);

					 if (ms.Length > 0)
						 r.Content = ms.ToArray();
				 }

				 return r;
			 });

			return result.Result;
		}

		public List<IBlobContent> Download(IServerResourceGroup resourceGroup, List<Guid> blobs)
		{
			throw new NotImplementedException();
		}

		public void Upload(IServerResourceGroup resourceGroup, Guid blob, byte[] content)
		{
			throw new NotImplementedException();
		}
	}
}
