﻿using System;
using TomPIT.Data.Sql;
using TomPIT.IoT;

namespace TomPIT.SysDb.Sql.IoT
{
	internal class IoTFieldState : PrimaryKeyRecord, IIoTFieldState
	{
		public string Field { get; set; }
		public string Value { get; set; }
		public DateTime Modified { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Field = GetString("field");
			Value = GetString("value");
			Modified = GetDate("modified");
		}
	}
}