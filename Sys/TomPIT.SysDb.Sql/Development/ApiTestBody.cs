using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class ApiTestBody : DatabaseRecord
	{
		public string Body { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Body = GetString("body");
		}
	}
}
