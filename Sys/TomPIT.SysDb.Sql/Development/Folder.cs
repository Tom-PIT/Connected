using System;
using System.ComponentModel;

using TomPIT.ComponentModel;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class Folder : PrimaryKeyRecord, IFolder
	{
		public string Name { get; set; }
      [Browsable(false)]
      public Guid Token { get; set; }
      [Browsable(false)]
      public Guid MicroService { get; set; }
      [Browsable(false)]
      public Guid Parent { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Token = GetGuid("token");
			MicroService = GetGuid("service_token");
			Parent = GetGuid("parent_token");
		}

      public override string ToString()
      {
         return string.IsNullOrWhiteSpace(Name)
             ? base.ToString()
             : Name;
      }
   }
}
