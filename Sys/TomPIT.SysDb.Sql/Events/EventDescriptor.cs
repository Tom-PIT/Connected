using System;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Events;

namespace TomPIT.SysDb.Sql.Events
{
	internal class EventDescriptor : LongPrimaryKeyRecord, IEventDescriptor
	{
		public Guid Identifier { get; set; }
		public string Arguments { get; set; }
		public string Name { get; set; }
		public DateTime Created { get; set; }
		public string Callback { get; set; }
		public Guid MicroService { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Identifier = GetGuid("identifier");

			if (IsDefined("arguments"))
				Arguments = GetString("arguments");

			Name = GetString("name");
			Created = GetDate("created");
			Callback = GetString("callback");
			MicroService = GetGuid("service");
		}
	}
}
