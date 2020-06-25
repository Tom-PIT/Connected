namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class SchemaSynchronize : SynchronizationTransaction
	{
		public SchemaSynchronize(ISynchronizer owner) : base(owner)
		{
		}

		protected override void OnExecute()
		{
			if (!new SchemaExists(Owner, Model.Schema).Execute())
				CreateSchema();
		}

		private void CreateSchema()
		{
			var command = Owner.CreateCommand();

			command.CommandText = $"CREATE SCHEMA {Model.Schema};";
			command.ExecuteNonQuery();
		}
	}
}
