using System;
using TomPIT.Api.Storage;

namespace TomPIT.StorageProvider.Azure
{
	public class AzureStorageProvider : IStorageProvider
	{
		private Lazy<Blobs> _blobs = new Lazy<Blobs>();

		public Guid Token { get { return new Guid("174D82E388E941379263112EA6FBA981"); } }

		public IBlobProvider Blobs { get { return _blobs.Value; } }

		public string Name => "Azure";
	}
}
