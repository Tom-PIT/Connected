using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class PrimaryKeyAdd : ColumnTransaction
	{
		public PrimaryKeyAdd(ISynchronizer owner, IModelSchemaColumn column) : base(owner, column)
		{

		}

		protected override void OnExecute()
		{
			Owner.CreateCommand(CommandText).ExecuteNonQuery();
		}
		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"ALTER TABLE {Escape(Model.SchemaName(), Model.Name)}");
				text.AppendLine($"ADD CONSTRAINT {Owner.GenerateConstraintName(ConstraintNameType.PrimaryKey)}");
				text.AppendLine($"PRIMARY KEY CLUSTERED ({Escape(Column.Name)}) ON {Escape(Owner.FileGroup())}");

				return text.ToString();
			}
		}
	}
}
