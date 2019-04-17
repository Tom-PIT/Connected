using System;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class MicroService : PrimaryKeyRecord, IMicroService
	{
		public string Name { get; set; }
		public string Url { get; set; }
		public Guid Token { get; set; }
		public MicroServiceStatus Status { get; set; }
		public Guid ResourceGroup { get; set; }
		public Guid Template { get; set; }
		public Guid Package { get; set; }
        public UpdateStatus UpdateStatus { get; set; }
        public CommitStatus CommitStatus { get; set; }
        public string Version { get; set; }

        protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Url = GetString("url");
			Token = GetGuid("token");
			Status = GetValue("status", MicroServiceStatus.Development);
			ResourceGroup = GetGuid("resource_token");
			Template = GetGuid("template");
			Package = GetGuid("package");
            UpdateStatus = GetValue("update_status", UpdateStatus.UpToDate);
            CommitStatus = GetValue("commit_status", CommitStatus.Synchronized);
            Version = GetString("version");
        }
	}
}
