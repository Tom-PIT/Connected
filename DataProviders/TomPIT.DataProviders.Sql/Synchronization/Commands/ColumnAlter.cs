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

			if (!string.IsNullOrWhiteSpace(ExistingColumn.DefaultValue))
			{
				var existingDefault = SqlDataProviderExtensions.ParseDefaultValue(ExistingColumn.DefaultValue);
				var def = SqlDataProviderExtensions.ParseDefaultValue(Column.DefaultValue);

				if (string.Compare(existingDefault, def, false) != 0)
					new DefaultDrop(Owner, Column).Execute();
			}
			if (Column.DataType != ExistingColumn.DataType
				|| Column.IsNullable != ExistingColumn.IsNullable
				|| Column.MaxLength != ExistingColumn.MaxLength
				|| Column.IsVersion != ExistingColumn.IsVersion)
				Owner.CreateCommand(CommandText).ExecuteNonQuery();

			var ed = SqlDataProviderExtensions.ParseDefaultValue(ExistingColumn.DefaultValue);
			var nd = SqlDataProviderExtensions.ParseDefaultValue(Column.DefaultValue);

			if (string.Compare(ed, nd, false) != 0 && nd != null)
				new DefaultAdd(Owner, Column, Model.Name).Execute();

			if (!ExistingColumn.IsPrimaryKey && Column.IsPrimaryKey)
				new PrimaryKeyAdd(Owner, Column).Execute();
			else if (ExistingColumn.IsPrimaryKey && !Column.IsPrimaryKey)
				new PrimaryKeyRemove(Owner, Existing, ExistingColumn).Execute();
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
