using System;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;
using TomPIT.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class Component : PrimaryKeyRecord, IComponent
	{
		public string Name { get; set; }
		public Guid MicroService { get; set; }
		public Guid Token { get; set; }
		public string Type { get; set; }
		public string Category { get; set; }
		public Guid RuntimeConfiguration { get; set; }
		public DateTime Modified { get; set; }
		public Guid Folder { get; set; }
		public LockStatus LockStatus { get; set; }
		public Guid LockUser { get; set; }
		public DateTime LockDate { get; set; }
		public LockVerb LockVerb { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			MicroService = GetGuid("service_token");
			Token = GetGuid("token");
			Type = GetString("type");
			Category = GetString("category");
			RuntimeConfiguration = GetGuid("runtime_configuration");
			Modified = GetDate("modified");
			Folder = GetGuid("folder_token");
			LockStatus = GetValue("lock_status", LockStatus.Commit);
			LockUser = GetGuid("lock_user_token");
			LockDate = GetDate("lock_date");
			LockVerb = GetValue("lock_verb", LockVerb.None);
		}
	}
}
