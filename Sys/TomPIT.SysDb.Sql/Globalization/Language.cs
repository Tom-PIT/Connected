using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using TomPIT.Data.Sql;
using TomPIT.Globalization;

namespace TomPIT.SysDb.Sql.Globalization
{
	internal class Language : PrimaryKeyRecord, ILanguage
	{
		[Required]
		public int Lcid { get; set; }
		[Required]
		public string Name { get; set; }
		public LanguageStatus Status { get; set; }
		public string Mappings { get; set; }
      [Browsable(false)]
      public Guid Token { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Lcid = GetInt("lcid");
			Name = GetString("name");
			Status = GetValue("status", LanguageStatus.Hidden);
			Mappings = GetString("mappings");
			Token = GetGuid("token");
		}

      public override string ToString()
      {
         return !string.IsNullOrWhiteSpace(Name)? Name : base.ToString();
      }
   }
}
