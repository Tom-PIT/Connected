using System;
using TomPIT.Api.Net;
using TomPIT.Data.Sql;

namespace TomPIT.StorageProvider.Sql
{
	internal class Message : LongPrimaryKeyRecord, IMessage
	{
		public string Text { get; set; }
		public string Topic { get; set; }
		public DateTime Created { get; set; }
		public DateTime Expire { get; set; }
		public TimeSpan RetryInterval { get; set; }
		public Guid Token { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Text = GetString("message");
			Topic = GetString("topic_name");
			Created = GetDate("created");
			Expire = GetDate("expire");
			RetryInterval = TimeSpan.FromSeconds(GetInt("retry_interval"));
			Token = GetGuid("token");
		}
	}
}
