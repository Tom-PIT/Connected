﻿using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Storage
{
	internal class BlobContentCache : SynchronizedClientRepository<IBlobContent, Guid>
	{
		public BlobContentCache(ITenant tenant) : base(tenant, "blobcontent")
		{

		}

		protected override void OnInitializing()
		{
		}

		public List<IBlobContent> Query(List<Guid> blobs)
		{
			var items = Instance.SysProxy.Storage.Download(blobs);
			var result = new List<IBlobContent>();

			foreach (var item in items)
			{
				var decompressed = BlobContentCompression.Decompress(item.Blob, item.Content);

				Set(item.Blob, decompressed);

				result.Add(decompressed);
			}

			return result;
		}

		public void Cache(IBlobContent content)
		{
			Set(content.Blob, content, TimeSpan.Zero);
		}

		public bool TrySelect(Guid blob, out IBlobContent content)
		{
			content = Get(blob);

			return content is not null;
		}
		public IBlobContent Select(IBlob blob)
		{
			return Get(blob.Token,
				(f) =>
				{
					var result = Instance.SysProxy.Storage.Download(blob.Token);

					if (result is null)
						return null;

					return BlobContentCompression.Decompress(result.Blob, result.Content);
				});
		}

		public void Delete(Guid blob)
		{
			Remove(blob);
		}
	}
}
