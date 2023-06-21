using System;
using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class Partition : PrimaryKeyRecord, IPartition
	{
		public Guid Configuration { get; set; }
		public int FileCount { get; set; }
		public PartitionStatus Status { get; set; }
		public string Name { get; set; }
		public DateTime Created { get; set; }
		public Guid ResourceGroup { get; set; }

      public override string ToString()
      {
         return string.IsNullOrWhiteSpace(Name) ? base.ToString() : Name;
      }

      protected override void OnCreate()
		{
			base.OnCreate();

			Configuration = GetGuid("configuration");
			FileCount = GetInt("file_count");
			Status = GetValue("status", PartitionStatus.Maintenance);
			Name = GetString("name");
			Created = GetDate("created");
			ResourceGroup = GetGuid("resource_group_token");
		}
	}
}
