using System;
using System.Collections.Generic;
using TomPIT.Api.Storage;
using TomPIT.Storage;
using TomPIT.SysDb.Environment;

namespace TomPIT.StorageProvider.Azure
{
	internal class Blobs : IBlobProvider
	{
		public void Delete(IServerResourceGroup resourceGroup, Guid blob)
		{
			//var ablob = AzureUtils.GetBlobReference(resourceGroup, AzureUtils.StorageContainer, blob.ToString());

			//Task.Run(async () =>
			//{
			//	if (!ablob.ExistsAsync().Result)
			//		return;

			//	await ablob.DeleteAsync();
			//}).Wait();
		}

		public IBlobContent Download(IServerResourceGroup resourceGroup, Guid blob)
		{
			return null;
			//var ablob = AzureUtils.GetBlobReference(resourceGroup, AzureUtils.StorageContainer, blob.ToString());

			//return Task.Run<IBlobContent>(async () =>
			// {
			//	 var exists = await ablob.ExistsAsync();

			//	 if (!exists)
			//		 return null;

			//	 var r = new BlobContent
			//	 {
			//		 Blob = blob
			//	 };

			//	 using (var ms = new MemoryStream())
			//	 {
			//		 await ablob.DownloadToStreamAsync(ms);

			//		 ms.Seek(0, SeekOrigin.Begin);

			//		 if (ms.Length > 0)
			//			 r.Content = ms.ToArray();
			//	 }

			//	 return r;
			// }).Result;
		}

		public List<IBlobContent> Download(IServerResourceGroup resourceGroup, List<Guid> blobs)
		{
			var result = new List<IBlobContent>();

			foreach (var blob in blobs)
			{
				var content = Download(resourceGroup, blob);

				if (content != null)
					result.Add(content);
			}

			return result;
		}

		public List<IBlobContent> Download(IServerResourceGroup resourceGroup, List<int> types)
		{
			throw new NotImplementedException();
		}

		public void Upload(IServerResourceGroup resourceGroup, Guid blob, byte[] content)
		{
			//var b = AzureUtils.GetBlobReference(resourceGroup, AzureUtils.StorageContainer, blob.ToString());

			////if (b != null)
			////	b.DeleteIfExistsAsync();

			//using (var stream = new MemoryStream(content, false))
			//{
			//	Task.Run(async () =>
			//	{
			//		await b.UploadFromStreamAsync(stream).ConfigureAwait(false);
			//	}).Wait();
			//}
		}
	}
}
