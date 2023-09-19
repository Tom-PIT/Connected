using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.BigData
{
   internal class Node : PrimaryKeyRecord, INode
   {
      [Required]
      public string Name { get; set; }
      [Required]
      public string ConnectionString { get; set; }
      [Required]
      public string AdminConnectionString { get; set; }
      [Browsable(false)]
      public Guid Token { get; set; }
      public NodeStatus Status { get; set; }
      public long Size { get; set; }

      public override string ToString()
      {
         return string.IsNullOrWhiteSpace(Name) ? base.ToString() : Name;
      }

      protected override void OnCreate()
      {
         base.OnCreate();

         Name = GetString("name");
         ConnectionString = GetString("connection_string");
         AdminConnectionString = GetString("admin_connection_string");
         Token = GetGuid("token");
         Status = GetValue("status", NodeStatus.Inactive);
         Size = GetLong("size");
      }
   }
}
