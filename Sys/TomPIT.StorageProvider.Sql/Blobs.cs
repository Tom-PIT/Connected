using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TomPIT.Api.Storage;
using TomPIT.Storage;
using TomPIT.SysDb.Environment;

namespace TomPIT.StorageProvider.Sql
{
	internal class Blobs : IBlobProvider
	{
		public void Delete(IServerResourceGroup resourceGroup, Guid blob)
		{
			var r = new ResourceGroupWriter(resourceGroup, "tompit.blob_content_del");

			r.CreateParameter("@blob", blob);

			r.Execute();
		}

		public IBlobContent Download(IServerResourceGroup resourceGroup, Guid blob)
		{
			var r = new ResourceGroupReader<BlobContent>(resourceGroup, "tompit.blob_content_sel");

			r.CreateParameter("@blob", blob);

			return r.ExecuteSingleRow();
		}

		public List<IBlobContent> Download(IServerResourceGroup resourceGroup, List<Guid> blobs)
		{
			var r = new ResourceGroupReader<BlobContent>(resourceGroup, "tompit.blob_content_que");

			var dt = new DataTable();

			dt.Columns.Add("token", typeof(Guid));

			foreach (var i in blobs)
				dt.Rows.Add(i);

			r.CreateParameter("@blobs", dt);

			return r.Execute().ToList<IBlobContent>();
		}


		public void Upload(IServerResourceGroup resourceGroup, Guid blob, byte[] content)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.blob_content_mdf");

			w.CreateParameter("@blob", blob);
			w.CreateParameter("@content", content, true);

			w.Execute();
		}
	}
}
