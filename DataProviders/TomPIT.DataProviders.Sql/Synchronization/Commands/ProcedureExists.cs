using System.Text;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class ProcedureExists : SynchronizationQuery<bool>
	{
		public ProcedureExists(ISynchronizer owner, string name) : base(owner)
		{
			Name = name;
		}
		private string Name { get; }
		protected override bool OnExecute()
		{
			return Types.Convert<bool>(Owner.CreateCommand(CommandText).ExecuteScalar());
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('{Name}')) SELECT 1; ELSE SELECT 0;");

				return text.ToString();
			}
		}
	}
}
