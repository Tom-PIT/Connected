using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using TomPIT.Data.Sql;
using TomPIT.Development;

using IComponent = TomPIT.ComponentModel.IComponent;

namespace TomPIT.SysDb.Sql.Development
{
    internal class Component : PrimaryKeyRecord, IComponent
	{
        [Required]
        [MaxLength(128)]
        public string Name { get; set; }
        [Browsable(false)]
        public Guid MicroService { get; set; }
        [Browsable(false)]
        public Guid Token { get; set; }
        [Browsable(false)]
        public string Type { get; set; }
        [Browsable(false)]
        public string Category { get; set; }
        [Browsable(false)]
        public Guid RuntimeConfiguration { get; set; }
        [Browsable(false)]
        public DateTime Modified { get; set; }
        [Browsable(false)]
        public Guid Folder { get; set; }
        [Browsable(false)]
        public LockStatus LockStatus { get; set; }
        [Browsable(false)]
        public Guid LockUser { get; set; }
        [Browsable(false)]
        public DateTime LockDate { get; set; }
        [Browsable(false)]
        public LockVerb LockVerb { get; set; }
        [Browsable(false)]
        public string NameSpace { get; set; }

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
			NameSpace = GetString("namespace");
		}

      public override string ToString()
      {
         if (string.IsNullOrWhiteSpace(Name))
            return base.ToString();

         return Name;
      }
   }
}
