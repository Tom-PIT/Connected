﻿using System;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Events;

namespace TomPIT.Sys.Model.Cdn
{
	internal class EventDescriptor : LongPrimaryKeyRecord, IEventDescriptor
	{
		public Guid Identifier { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public string Arguments { get; set; }

		public string Callback { get; set; }

		public Guid MicroService { get; set; }
	}
}