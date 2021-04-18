using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class DefaultAdd : ColumnTransaction
	{
		public DefaultAdd(ISynchronizer owner, IModelSchemaColumn column, string tableName) : base(owner, column)
		{
			TableName = tableName;
		}

		private string TableName { get; }

		protected override void OnExecute()
		{
			Owner.CreateCommand(CommandText).ExecuteNonQuery();
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				var defValue = SqlDataProviderExtensions.ParseDefaultValue(Column.DefaultValue);

				text.AppendLine($"ALTER TABLE {Escape(Model.SchemaName(), TableName)}");
				text.AppendLine($"ADD CONSTRAINT {Owner.GenerateConstraintName(ConstraintNameType.Default)} DEFAULT {defValue} FOR {Column.Name}");

				return text.ToString();
			}
		}
	}
}
