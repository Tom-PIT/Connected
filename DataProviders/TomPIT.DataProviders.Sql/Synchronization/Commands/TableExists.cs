using System.Text;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class TableExists : SynchronizationQuery<bool>
	{
		public TableExists(ISynchronizer owner) : base(owner)
		{
		}

		protected override bool OnExecute()
		{
			return Types.Convert<bool>(Owner.CreateCommand(CommandText).ExecuteScalar());
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"IF (EXISTS (SELECT *  FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = '{Unescape(Model.SchemaName())}' AND TABLE_NAME = '{Unescape(Model.Name)}')) SELECT 1 ELSE SELECT 0");

				return text.ToString();
			}
		}
	}
}
