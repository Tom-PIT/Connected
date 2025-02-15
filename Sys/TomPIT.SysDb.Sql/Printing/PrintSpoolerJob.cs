﻿using System;
using TomPIT.Cdn;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Printing
{
	internal class PrintSpoolerJob : LongPrimaryKeyRecord, IPrintSpoolerJob
	{
		public Guid Token { get; set; }

		public Guid? Identity { get; set; }

		public string Mime { get; set; }

		public string Content { get; set; }

		public string Printer { get; set; }

		public int CopyCount { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Token = GetGuid("token");
			Mime = GetString("mime");
			Content = Convert.ToBase64String(GetValue<byte[]>("content", null));
			Printer = GetString("printer");
			Identity = GetGuid("identity");
			CopyCount = GetInt("copy_count");
		}
	}
}
