using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class ColumnDrop : ColumnTransaction
	{
		public ColumnDrop(ISynchronizer owner, IModelSchemaColumn column, ExistingModel existing) : base(owner, column)
		{
			Existing = existing;
		}

		private ExistingModel Existing { get; }

		protected override void OnExecute()
		{
			if (!string.IsNullOrWhiteSpace(Column.DefaultValue))
				new DefaultDrop(Owner, Column).Execute();

			var indexes = Existing.ResolveIndexes(Column.Name);

			foreach (var index in indexes)
				new IndexDrop(Owner, index).Execute();

			Owner.CreateCommand(CommandText).ExecuteNonQuery();
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"ALTER TABLE {Escape(Model.SchemaName(), Model.Name)} DROP COLUMN {Escape(Column.Name)};");

				return text.ToString();
			}
		}
	}
}
