using System;
using TomPIT.Api.Net;
using TomPIT.Data.Sql;

namespace TomPIT.StorageProvider.Sql
{
	internal class Recipient : LongPrimaryKeyRecord, IRecipient
	{
		public string Connection { get; set; }
		public Guid Message { get; set; }
		public string Topic { get; set; }
		public int RetryCount { get; set; }
		public DateTime NextVisible { get; set; }
		public string Content { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Connection = GetString("connection");
			Message = GetGuid("message_token");
			Topic = GetString("topic_name");
			RetryCount = GetInt("retry_count");
			NextVisible = GetDate("next_visible");
			Content = GetString("content");
		}
	}
}
