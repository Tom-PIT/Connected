using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class DefaultDrop : ColumnTransaction
	{
		public DefaultDrop(ISynchronizer owner, IModelSchemaColumn column) : base(owner, column)
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
				text.AppendLine($"DROP CONSTRAINT DF_{Unescape(Model.SchemaName())}_{Unescape(Model.Name)}_{Column.Name};");

				return text.ToString();
			}
		}
	}
}
