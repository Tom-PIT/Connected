using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using TomPIT.SysDb.Environment;

namespace TomPIT.StorageProvider.Azure
{
	internal static class AzureUtils
	{
		public const string StorageContainer = "tompitstorage";

		public static CloudBlockBlob GetBlobReference(IServerResourceGroup resourceGroup, string container, string name)
		{
			var blobClient = CreateStorageAccount(resourceGroup).CreateCloudBlobClient();
			var c = blobClient.GetContainerReference(container);

			Task.Run(() =>
			{
				if (!c.ExistsAsync().Result)
					c.CreateAsync();
			}
			);

			return c.GetBlockBlobReference(name);
		}

		public static CloudStorageAccount CreateStorageAccount(IServerResourceGroup resourceGroup)
		{
			return CloudStorageAccount.Parse(resourceGroup.ConnectionString);
		}
	}
}
