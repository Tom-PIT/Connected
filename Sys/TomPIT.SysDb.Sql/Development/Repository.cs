using TomPIT.Data.Sql;
using TomPIT.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class Repository : PrimaryKeyRecord, IRepositoriesEndpoint
	{
		public string Name { get; set; }

		public string Url { get; set; }

		public string UserName { get; set; }

		public byte[] Password { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Url = GetString("url");
			UserName = GetString("user_name");
			Password = GetValue<byte[]>("password", null);
		}
	}
}
