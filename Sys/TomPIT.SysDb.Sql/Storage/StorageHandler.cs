using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Environment;
using TomPIT.Storage;
using TomPIT.SysDb.Storage;

namespace TomPIT.SysDb.Sql.Storage
{
	internal class StorageHandler : IStorageHandler
	{
		public List<IBlob> QueryOrphaned(DateTime modified)
		{
			using var r = new Reader<Blob>("tompit.blob_que_draft_orphaned");

			r.CreateParameter("@modified", modified);

			return r.Execute().ToList<IBlob>();
		}

		public void Commit(string draft, string primaryKey)
		{
			using var w = new Writer("tompit.blob_commit");

			w.CreateParameter("@primary_key", primaryKey);
			w.CreateParameter("@draft", draft);

			w.Execute();
		}

		public void Delete(IBlob blob)
		{
			using var w = new Writer("tompit.blob_del");

			w.CreateParameter("@id", blob.GetId());

			w.Execute();
		}

		public IBlob Select(Guid token)
		{
			using var r = new Reader<Blob>("tompit.blob_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public List<IBlob> Query(IResourceGroup resourceGroup, int type, string primaryKey)
		{
			using var r = new Reader<Blob>("tompit.blob_que");

			r.CreateParameter("@resource_group", resourceGroup.GetId());
			r.CreateParameter("@type", type);
			r.CreateParameter("@primary_key", primaryKey);

			return r.Execute().ToList<IBlob>();
		}

		public List<IBlob> Query(IResourceGroup resourceGroup, int type, string primaryKey, Guid microService, string topic)
		{
			using var r = new Reader<Blob>("tompit.blob_que");

			r.CreateParameter("@resource_group", resourceGroup.GetId());
			r.CreateParameter("@type", type);
			r.CreateParameter("@primary_key", primaryKey);
			r.CreateParameter("@service", microService, true);
			r.CreateParameter("@topic", topic, true);

			return r.Execute().ToList<IBlob>();
		}

		public List<IBlob> Query(Guid microService)
		{
			using var r = new Reader<Blob>("tompit.blob_que");

			r.CreateParameter("@service", microService);

			return r.Execute().ToList<IBlob>();
		}

		public List<IBlob> Query(Guid microService, int type)
		{
			using var r = new Reader<Blob>("tompit.blob_que");

			r.CreateParameter("@service", microService, true);
			r.CreateParameter("@type", type);

			return r.Execute().ToList<IBlob>();
		}

		public List<IBlob> QueryDrafts(string draft)
		{
			using var r = new Reader<Blob>("tompit.blob_que_draft");

			r.CreateParameter("@draft", draft);

			return r.Execute().ToList<IBlob>();
		}

		public void Insert(IResourceGroup resourceGroup, Guid token, int type, string primaryKey,
			Guid microService, string topic, string fileName, string contentType, int size, int version, DateTime modified, string draft)
		{
			using var w = new Writer("tompit.blob_ins");

			w.CreateParameter("@resource_group", resourceGroup.GetId());
			w.CreateParameter("@file_name", fileName, true);
			w.CreateParameter("@token", token);
			w.CreateParameter("@size", size);
			w.CreateParameter("@type", type);
			w.CreateParameter("@content_type", contentType, true);
			w.CreateParameter("@primary_key", primaryKey, true);
			w.CreateParameter("@service", microService, true);
			w.CreateParameter("@draft", draft, true);
			w.CreateParameter("@version", version);
			w.CreateParameter("@topic", topic, true);
			w.CreateParameter("@modified", modified);

			w.Execute();
		}

		public void Update(IBlob blob, string primaryKey, string fileName, string contentType, int size, int version, DateTime modified, string draft)
		{
			using var w = new Writer("tompit.blob_upd");

			w.CreateParameter("@file_name", fileName, true);
			w.CreateParameter("@id", blob.GetId());
			w.CreateParameter("@size", size);
			w.CreateParameter("@content_type", contentType, true);
			w.CreateParameter("@primary_key", primaryKey, true);
			w.CreateParameter("@draft", draft, true);
			w.CreateParameter("@version", version);
			w.CreateParameter("@modified", modified);

			w.Execute();
		}

		public List<IBlob> Query(List<Guid> blobs)
		{
			using var r = new Reader<Blob>("tompit.blob_list");
			var dt = new DataTable();

			dt.Columns.Add("token", typeof(Guid));

			foreach (var i in blobs)
				dt.Rows.Add(i);

			r.CreateParameter("@items", dt);

			return r.Execute().ToList<IBlob>();
		}

		public List<IBlob> QueryByLevel(Guid microService, int level)
		{
			using var r = new Reader<Blob>("tompit.blob_level_que");

			r.CreateParameter("@service", microService);
			r.CreateParameter("@level", level);

			return r.Execute().ToList<IBlob>();
		}
	}
}
