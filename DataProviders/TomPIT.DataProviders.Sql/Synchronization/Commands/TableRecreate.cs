using System.Linq;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class TableRecreate : TableTransaction
	{
		public TableRecreate(ISynchronizer owner, ExistingModel existing) : base(owner)
		{
			Existing = existing;
		}

		private ExistingModel Existing { get; }

		protected override void OnExecute()
		{
			var add = new TableCreate(Owner, true);

			add.Execute();

			ExecuteDefaults(add.TemporaryName);

			if (HasIdentity)
				new IdentityInsert(Owner, add.TemporaryName, true).Execute();

			new DataCopy(Owner, Existing, add.TemporaryName).Execute();

			if (HasIdentity)
				new IdentityInsert(Owner, add.TemporaryName, false).Execute();

			new TableDrop(Owner).Execute();
			new TableRename(Owner, add.TemporaryName).Execute();

			ExecutePrimaryKey();
			ExecuteIndexes();
		}

		private bool HasIdentity
		{
			get
			{
				foreach (var column in Model.Columns)
				{
					if (column.IsPrimaryKey && column.IsIdentity)
						return true;
				}

				return false;
			}
		}

		private void ExecutePrimaryKey()
		{
			var pk = Model.Columns.FirstOrDefault(f => f.IsPrimaryKey);

			if (pk != null)
				new PrimaryKeyAdd(Owner, pk).Execute();
		}

		private void ExecuteDefaults(string tableName)
		{
			foreach (var column in Owner.Model.Columns)
			{
				if (!string.IsNullOrWhiteSpace(column.DefaultValue))
					new DefaultAdd(Owner, column, tableName).Execute();
			}
		}

		private void ExecuteIndexes()
		{
			var indexes = ParseIndexes(Owner.Model);

			foreach (var index in indexes)
				new IndexCreate(Owner, index).Execute();
		}
	}
}
