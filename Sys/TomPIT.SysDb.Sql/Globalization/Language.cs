using System;
using TomPIT.Data.Sql;
using TomPIT.Globalization;

namespace TomPIT.SysDb.Sql.Globalization
{
	internal class Language : PrimaryKeyRecord, ILanguage
	{
		public int Lcid { get; set; }
		public string Name { get; set; }
		public LanguageStatus Status { get; set; }
		public string Mappings { get; set; }
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
	}
}
