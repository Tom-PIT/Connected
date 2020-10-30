using System.Text;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class TableDrop : TableTransaction
	{
		public TableDrop(ISynchronizer owner) : base(owner)
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

				text.AppendLine($"DROP TABLE {Escape(Model.SchemaName(), Model.Name)}");

				return text.ToString();
			}
		}
	}
}
