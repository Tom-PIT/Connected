using System.Text;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class IdentityInsert : TableTransaction
	{
		public IdentityInsert(ISynchronizer owner, string tableName, bool on) : base(owner)
		{
			On = on;
			TableName = tableName;
		}

		private string TableName { get; }
		private bool On { get; }

		protected override void OnExecute()
		{
			Owner.CreateCommand(CommandText).ExecuteNonQuery();
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();
				var switchCommand = On ? "ON" : "OFF";

				text.AppendLine($"SET IDENTITY_INSERT {Escape(Model.SchemaName(), TableName)}  {switchCommand}");

				return text.ToString();
			}
		}
	}
}
