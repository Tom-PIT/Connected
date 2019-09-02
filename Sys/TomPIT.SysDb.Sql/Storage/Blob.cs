using System;
using TomPIT.Data.Sql;
using TomPIT.Storage;

namespace TomPIT.SysDb.Sql.Storage
{
	public class Blob : LongPrimaryKeyRecord, IBlob
	{
		public const string Png = "image/png";

		public string FileName { get; set; }
		public Guid Token { get; private set; }
		public int Size { get; private set; }
		public string ContentType { get; set; } = Png;
		public string PrimaryKey { get; set; }
		public Guid MicroService { get; set; }
		public string Draft { get; set; }
		public int Version { get; private set; }
		public int Type { get; set; }
		public string Topic { get; set; }
		public Guid ResourceGroup { get; set; }
		public DateTime Modified { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			ResourceGroup = GetGuid("resource_token");
			FileName = GetString("file_name");
			Token = GetGuid("token");
			Size = GetInt("size");
			Type = GetInt("type");
			ContentType = GetString("content_type");
			PrimaryKey = GetString("primary_key");
			MicroService = GetGuid("service");
			Draft = GetString("draft");
			Version = GetInt("version");
			Topic = GetString("topic");
			Modified = GetDate("modified");
		}
	}
}
