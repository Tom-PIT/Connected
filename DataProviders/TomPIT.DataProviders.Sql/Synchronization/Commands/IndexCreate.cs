using System.Text;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class IndexCreate : TableTransaction
	{
		public IndexCreate(ISynchronizer owner, IndexDescriptor index) : base(owner)
		{
			Index = index;
		}

		private IndexDescriptor Index { get; }

		protected override void OnExecute()
		{
			if (Index.Unique)
				new UniqueConstraintAdd(Owner, Index).Execute();
			else
				Owner.CreateCommand(CommandText).ExecuteNonQuery();
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"CREATE NONCLUSTERED INDEX [{Owner.GenerateConstraintName(ConstraintNameType.Index)}] ON {Escape(Model.SchemaName(), Model.Name)}(");
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
