using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class ColumnAlter : ColumnTransaction
	{
		public ColumnAlter(ISynchronizer owner, IModelSchemaColumn column, ExistingModel existing, IModelSchemaColumn existingColumn) : base(owner, column)
		{
			Existing = existing;
			ExistingColumn = existingColumn;
		}

		private ExistingModel Existing { get; }
		private IModelSchemaColumn ExistingColumn { get; }

		protected override void OnExecute()
		{
			if (ExistingColumn.Equals(Column))
				return;

			if (!string.IsNullOrWhiteSpace(ExistingColumn.DefaultValue)
				&& string.Compare(ExistingColumn.DefaultValue, Column.DefaultValue, false) != 0)
				new DefaultDrop(Owner, Column);

			if (Column.DataType != ExistingColumn.DataType
				|| Column.IsNullable != ExistingColumn.IsNullable
				|| Column.MaxLength != ExistingColumn.MaxLength)
				Owner.CreateCommand(CommandText).ExecuteNonQuery();

			if (string.Compare(ExistingColumn.DefaultValue, Column.DefaultValue, false) != 0)
				new DefaultAdd(Owner, Column);

			if (!ExistingColumn.IsPrimaryKey && Column.IsPrimaryKey)
				new PrimaryKeyAdd(Owner, Column);
			else if (ExistingColumn.IsPrimaryKey && !Column.IsPrimaryKey)
				new PrimaryKeyRemove(Owner, Existing, ExistingColumn);
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"ALTER TABLE {Escape(Model.SchemaName(), Model.Name)}");
				text.AppendLine($"ALTER COLUMN {CreateColumnCommandText(Column)}");

				return text.ToString();
			}
		}
	}
}
