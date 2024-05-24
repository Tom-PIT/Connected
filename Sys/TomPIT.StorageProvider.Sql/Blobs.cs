using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TomPIT.Api.Storage;
using TomPIT.Environment;
using TomPIT.Storage;

namespace TomPIT.StorageProvider.Sql
{
    internal class Blobs : IBlobProvider
    {
        public void Delete(IServerResourceGroup resourceGroup, Guid blob)
        {
            using var r = new ResourceGroupWriter(resourceGroup, "tompit.blob_content_del");

            r.CreateParameter("@blob", blob);

            r.Execute();
        }

        public List<IBlobContent> Download(IServerResourceGroup resourceGroup, List<int> types)
        {
            using var r = new ResourceGroupReader<BlobContent>(resourceGroup, "tompit.blob_content_que_by_type");

            var ja = new JArray();

            foreach (var type in types)
            {
                ja.Add(new JObject
                {
                    { "type", type }
                });
            }

            r.CreateParameter("@resource_group", resourceGroup.GetId());
            r.CreateParameter("@types", ja);

            return r.Execute().ToList<IBlobContent>();
        }
        public IBlobContent Download(IServerResourceGroup resourceGroup, Guid blob)
        {
            using var r = new ResourceGroupReader<BlobContent>(resourceGroup, "tompit.blob_content_sel");

            r.CreateParameter("@blob", blob);

            return r.ExecuteSingleRow();
        }

        public List<IBlobContent> Download(IServerResourceGroup resourceGroup, List<Guid> blobs)
        {
            using var r = new ResourceGroupReader<BlobContent>(resourceGroup, "tompit.blob_content_que");

            var dt = new DataTable();

            dt.Columns.Add("token", typeof(Guid));

            foreach (var i in blobs)
                dt.Rows.Add(i);

            r.CreateParameter("@blobs", dt);

            return r.Execute().ToList<IBlobContent>();
        }


        public void Upload(IServerResourceGroup resourceGroup, Guid blob, byte[] content)
        {
            using var w = new ResourceGroupWriter(resourceGroup, "tompit.blob_content_mdf");

            w.CreateParameter("@blob", blob);
            w.CreateParameter("@content", content, true);

            w.Execute();
        }
    }
}
