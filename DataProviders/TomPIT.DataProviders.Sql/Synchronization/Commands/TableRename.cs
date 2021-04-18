using System.Text;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class TableRename : TableTransaction
	{
		public TableRename(ISynchronizer owner, string temporaryName) : base(owner)
		{
			TemporaryName = temporaryName;
		}
		private string TemporaryName { get; }

		protected override void OnExecute()
		{
			Owner.CreateCommand(CommandText).ExecuteNonQuery();
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"EXECUTE sp_rename N'{Unescape(Model.SchemaName())}.{Unescape(TemporaryName)}', N'{Unescape(Model.Name)}', 'OBJECT'");

				return text.ToString();
			}
		}
	}
}
