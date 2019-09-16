using System;
using System.Collections.Generic;
using System.Linq;
using LZ4;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Storage
{
	internal class BlobContentCache : ClientRepository<IBlobContent, Guid>
	{
		public BlobContentCache(ITenant tenant) : base(tenant, "blobcontent")
		{

		}

		public List<IBlobContent> Query(List<Guid> blobs)
		{
			var u = Tenant.CreateUrl("Storage", "DownloadBatch");
			var args = new JObject();
			var a = new JArray();

			foreach (var i in blobs)
				a.Add(i);

			args.Add("items", a);

			var ds = Tenant.Post<List<BlobContent>>(u, args);

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
					var u = Tenant.CreateUrl("Storage", "Download")
					.AddParameter("resourceGroup", blob.ResourceGroup)
					.AddParameter("microService", blob.MicroService)
					.AddParameter("blob", blob.Token);

					var r = Tenant.Get<BlobContent>(u);

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
