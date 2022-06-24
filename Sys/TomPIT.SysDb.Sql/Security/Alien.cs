using System;
using TomPIT.Data.Sql;
using TomPIT.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class Alien : LongPrimaryKeyRecord, IAlien
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Mobile { get; set; }
		public string Phone { get; set; }
		public Guid Token { get; set; }
		public Guid Language { get; set; }
		public string TimeZone { get; set; }
		public string ResourceType {get;set;}
		public string ResourcePrimaryKey {get;set;}

		protected override void OnCreate()
		{
			base.OnCreate();

			FirstName = GetString("first_name");
			LastName = GetString("last_name");
			Email = GetString("email");
			Mobile = GetString("mobile");
			Phone = GetString("phone");
			Token = GetGuid("token");
			Language = GetGuid("language_token");
			TimeZone = GetString("timezone");
			ResourceType = GetString("resource_type");
			ResourcePrimaryKey = GetString("resource_primary_key");
		}
	}
}
