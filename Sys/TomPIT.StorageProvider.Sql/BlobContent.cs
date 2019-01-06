using System;
using TomPIT.Data.Sql;
using TomPIT.Storage;

namespace TomPIT.StorageProvider.Sql
{
	internal class BlobContent : LongPrimaryKeyRecord, IBlobContent
	{
		public byte[] Content { get; set; }
		public Guid Blob { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Content = GetValue<byte[]>("content", null);
			Blob = GetGuid("blob");
		}
	}
}
