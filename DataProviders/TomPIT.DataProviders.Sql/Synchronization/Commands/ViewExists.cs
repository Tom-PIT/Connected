using System.Text;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class ViewExists : SynchronizationQuery<bool>
	{
		public ViewExists(ISynchronizer owner, string name) : base(owner)
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

				text.AppendLine($"IF EXISTS (SELECT * FROM sys.objects WHERE type = 'V' AND OBJECT_ID = OBJECT_ID('{Name}')) SELECT 1; ELSE SELECT 0;");

				return text.ToString();
			}
		}
	}
}
