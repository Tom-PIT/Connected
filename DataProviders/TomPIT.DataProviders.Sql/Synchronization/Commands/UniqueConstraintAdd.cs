using System.Text;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class UniqueConstraintAdd : TableTransaction
	{
		public UniqueConstraintAdd(ISynchronizer owner, IndexDescriptor index) : base(owner)
		{
			Index = index;
		}

		private IndexDescriptor Index { get; }

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
				text.AppendLine($"ADD CONSTRAINT [{Owner.GenerateConstraintName(ConstraintNameType.Index)}] UNIQUE NONCLUSTERED (");
				var comma = string.Empty;

				foreach (var column in Index.Columns)
				{
					text.AppendLine($"{comma}{Escape(column)} ASC");

					comma = ",";
				}

				text.AppendLine($") ON {Escape(Owner.FileGroup())}");

				return text.ToString();
			}
		}
	}
}
