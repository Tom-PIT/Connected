using System.Data.SqlClient;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class SchemaSynchronizer : SynchronizerBase
	{
		public SchemaSynchronizer(SqlCommand command, IModelSchema schema) : base(command, schema)
		{

		}

		protected override void OnExecute()
		{
			if (!SchemaExists())
				CreateSchema();
		}

		private bool SchemaExists()
		{
			if (string.IsNullOrWhiteSpace(Schema.Schema))
				return true;

			Command.CommandText = string.Format("SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'", Schema.Schema);

			var rdr = Command.ExecuteReader();

			var r = rdr.HasRows;

			rdr.Close();

			return r;
		}

		private void CreateSchema()
		{
			Command.CommandText = $"CREATE SCHEMA {SchemaName}";
			Command.ExecuteNonQuery();
		}
	}
}
