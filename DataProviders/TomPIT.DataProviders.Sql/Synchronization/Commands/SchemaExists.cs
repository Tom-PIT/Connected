namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class SchemaExists : SynchronizationQuery<bool>
	{
		public SchemaExists(ISynchronizer owner, string name) : base(owner)
		{
			Name = name;
		}
		private string Name { get; }

		protected override bool OnExecute()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return true;

			var command = Owner.CreateCommand();

			command.CommandText = string.Format("SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'", Name);

			var rdr = command.ExecuteReader();

			var r = rdr.HasRows;

			rdr.Close();

			return r;
		}
	}
}
