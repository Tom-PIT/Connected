using System;
using System.Collections.Generic;
using System.Linq;
using LZ4;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Middleware;

namespace TomPIT.Storage
{
	internal class BlobContentCache : SynchronizedClientRepository<IBlobContent, Guid>
	{
		public BlobContentCache(ITenant tenant) : base(tenant, "blobcontent")
		{

		}

		protected override void OnInitializing()
		{
			var u = Tenant.CreateUrl("Storage", "DownloadByTypes");
			var args = new JObject();

			var resourceGroups = Tenant.GetService<IResourceGroupService>().Query();
			var ja = new JArray();
			var types = new JArray
			{
				BlobTypes.Template,
				BlobTypes.Configuration,
				BlobTypes.RuntimeConfiguration
			};

			foreach (var rg in resourceGroups)
				ja.Add(rg.Token);

			args.Add("resourceGroups", ja);
			args.Add("types", types);

			var contents = Tenant.Post<List<BlobContent>>(u, args);

			foreach (var content in contents)
			{
				if (content.Content != null && content.Content.Length > 0)
					content.Content = LZ4Codec.Unwrap(content.Content);

				Set(content.Blob, content, TimeSpan.Zero);
			}
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

		public void Cache(IBlobContent content)
		{
			Set(content.Blob, content, TimeSpan.Zero);
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
