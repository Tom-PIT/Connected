using System;
using TomPIT.Api.Net;
using TomPIT.Data.Sql;

namespace TomPIT.StorageProvider.Sql
{
	internal class Subscriber : LongPrimaryKeyRecord, ISubscriber
	{
		public string Topic { get; set; }
		public string Connection { get; set; }
		public DateTime Alive { get; set; }
		public DateTime Created { get; set; }
		public Guid Instance { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Topic = GetString("topic_name");
			Connection = GetString("connection");
			Alive = GetDate("alive");
			Created = GetDate("created");
			Instance = GetGuid("instance");
		}

	}
}
