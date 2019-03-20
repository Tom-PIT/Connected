using System;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
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
