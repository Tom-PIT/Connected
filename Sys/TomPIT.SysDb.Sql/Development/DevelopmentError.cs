﻿using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Data.Sql;
using TomPIT.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class DevelopmentError : LongPrimaryKeyRecord, IDevelopmentError
	{
		public Guid MicroService { get; set; }
		public Guid Component { get; set; }
		public Guid Element { get; set; }
		public DevelopmentSeverity Severity { get; set; }
		public string Message { get; set; }
		public string ComponentName { get; set; }
		public string ComponentCategory { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			MicroService = GetGuid("service_token");
			Component = GetGuid("component_token");
			Element = GetGuid("element");
			Severity = GetValue("severity", DevelopmentSeverity.Warning);
			Message = GetString("message");
			ComponentName = GetString("component_name");
			ComponentCategory = GetString("component_category");
		}
	}
}