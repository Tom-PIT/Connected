using System;
using TomPIT.Data.Sql;
using TomPIT.Environment;

namespace TomPIT.SysDb.Sql.Environment
{
	internal class Client : LongPrimaryKeyRecord, IClient
	{
		public string Token { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public ClientStatus Status { get; set; }

		public string Type { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Token = GetString("token");
			Name = GetString("name");
			Created = GetDate("created");
			Status = GetValue("status", ClientStatus.Enabled);
			Type = GetString("type");
		}
	}
}
