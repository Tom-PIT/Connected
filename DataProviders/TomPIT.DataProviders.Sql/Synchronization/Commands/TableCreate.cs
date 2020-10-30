using System;
using System.Linq;
using System.Text;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class TableCreate : TableTransaction
	{
		public TableCreate(ISynchronizer owner, bool temporary) : base(owner)
		{
			Temporary = temporary;

			if (Temporary)
				TemporaryName = $"T{Guid.NewGuid().ToString().Replace("-", string.Empty)}";
		}

		private bool Temporary { get; }

		public string TemporaryName { get; }
		protected override void OnExecute()
		{
			Owner.CreateCommand(CommandText).ExecuteNonQuery();

			if (!Temporary)
			{
				ExecutePrimaryKey();
				ExecuteDefaults();
				ExecuteIndexes();
			}
		}

		private void ExecutePrimaryKey()
		{
			var primaryKey = Model.Columns.FirstOrDefault(f => f.IsPrimaryKey);

			if (primaryKey != null)
				new PrimaryKeyAdd(Owner, primaryKey).Execute();
		}

		private void ExecuteDefaults()
		{
			var name = Temporary ? TemporaryName : Model.Name;

			foreach (var column in Owner.Model.Columns)
			{
				if (!string.IsNullOrWhiteSpace(column.DefaultValue))
					new DefaultAdd(Owner, column, name).Execute();
			}
		}

		private void ExecuteIndexes()
		{
			var indexes = ParseIndexes(Owner.Model);

			foreach (var index in indexes)
				new IndexCreate(Owner, index).Execute();
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				var name = Temporary ? TemporaryName : Model.Name;

				text.AppendLine($"CREATE TABLE {Escape(Model.SchemaName(), name)}");
				text.AppendLine("(");
				var comma = string.Empty;

				for (var i = 0; i < Model.Columns.Count; i++)
				{
					text.AppendLine($"{comma} {CreateColumnCommandText(Model.Columns[i])}");

					comma = ",";
				}

				text.AppendLine(");");

				return text.ToString();
			}
		}
	}
}
