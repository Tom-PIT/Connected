using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace TomPIT.StorageProvider.Azure
{
	internal static class AzureUtils
	{
		public const string StorageContainer = "tompitstorage";

		public static CloudBlockBlob GetBlobReference(string container, string name)
		{
			var blobClient = CreateStorageAccount().CreateCloudBlobClient();
			var c = blobClient.GetContainerReference(container);

			Task.Run(() =>
			{
				if (!c.ExistsAsync().Result)
					c.CreateAsync();
			}
			);

			return c.GetBlockBlobReference(name);
		}

		public static CloudStorageAccount CreateStorageAccount()
		{
			return CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("storage"));
		}
	}
}
