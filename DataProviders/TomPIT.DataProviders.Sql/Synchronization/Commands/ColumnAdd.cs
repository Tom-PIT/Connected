using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class ColumnAdd : ColumnTransaction
	{
		public ColumnAdd(ISynchronizer owner, IModelSchemaColumn column) : base(owner, column)
		{
		}

		protected override void OnExecute()
		{
			Owner.CreateCommand(CommandText).ExecuteNonQuery();

			if (Column.IsPrimaryKey)
				new PrimaryKeyAdd(Owner, Column).Execute();

			if (!string.IsNullOrWhiteSpace(Column.DefaultValue))
				new DefaultAdd(Owner, Column, Model.Name).Execute();
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"ALTER TABLE {Escape(Model.SchemaName(), Model.Name)}");
				text.AppendLine($"ADD COLUMN {CreateColumnCommandText(Column)}");

				return text.ToString();
			}
		}
	}
}
