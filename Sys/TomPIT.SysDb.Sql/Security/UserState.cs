using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Security
{
	internal class UserState : DatabaseRecord
	{
		public byte[] Content { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Content = GetValue<byte[]>("state", null);
		}
	}
}
