using LZ4;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Net;

namespace TomPIT.Storage
{
	internal class BlobContentCache : ContextCacheRepository<IBlobContent, Guid>
	{
		public BlobContentCache(ISysContext server) : base(server, "blobcontent")
		{

		}

		public List<IBlobContent> Query(List<Guid> blobs)
		{
			var u = Server.CreateUrl("Storage", "DownloadBatch");
			var args = new JObject();
			var a = new JArray();

			foreach (var i in blobs)
				a.Add(i);

			args.Add("items", a);

			var ds = Server.Connection.Post<List<BlobContent>>(u, args);

			foreach (var i in ds)
			{
				i.Content = LZ4Codec.Unwrap(i.Content);
				Set(i.Blob, i);
			}

			return ds.ToList<IBlobContent>();
		}

		public IBlobContent Select(IBlob blob)
		{
			return Get(blob.Token,
				(f) =>
				{
					var u = Server.CreateUrl("Storage", "Download")
					.AddParameter("resourceGroup", blob.ResourceGroup)
					.AddParameter("microService", blob.MicroService)
					.AddParameter("blob", blob.Token);

					var r = Server.Connection.Get<BlobContent>(u);

					if (r != null)
						r.Content = LZ4Codec.Unwrap(r.Content);

					return r;
				});
		}

		public void Delete(Guid blob)
		{
			Remove(blob);
		}
	}
}
