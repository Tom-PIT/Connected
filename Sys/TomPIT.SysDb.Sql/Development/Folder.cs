using System;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class Folder : PrimaryKeyRecord, IFolder
	{
		public string Name { get; set; }
		public Guid Token { get; set; }
		public Guid MicroService { get; set; }
		public Guid Parent { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Token = GetGuid("token");
			MicroService = GetGuid("service_token");
			Parent = GetGuid("parent_token");
		}
	}
}
