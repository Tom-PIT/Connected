using TomPIT.Data.Sql;
using TomPIT.Security;

namespace TomPIT.SysDb.Sql.Data
{
	internal class UserData : LongPrimaryKeyRecord, IUserData
	{
		public string Topic { get; set; }
		public string PrimaryKey { get; set; }
		public string Value { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Topic = GetString("topic");
			PrimaryKey = GetString("primary_key");
			Value = GetString("value");
		}
	}
}
