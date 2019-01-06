using TomPIT.Data.Sql;
using TomPIT.SysDb.Environment;

namespace TomPIT.SysDb.Sql.Environment
{
	internal class EnvironmentVariable : DatabaseRecord, IEnvironmentVariable
	{
		public string Name { get; set; }
		public string Value { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Value = GetString("value");
		}
	}
}
