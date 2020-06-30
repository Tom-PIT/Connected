using System.Text;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class IndexDrop : TableTransaction
	{
		public IndexDrop(ISynchronizer owner, ObjectIndex index) : base(owner)
		{
			Index = index;
		}

		private ObjectIndex Index { get; }

		protected override void OnExecute()
		{
			switch (Index.Type)
			{
				case IndexType.Index:
					Owner.CreateCommand(CommandText).ExecuteNonQuery();
					break;
				case IndexType.Unique:
				case IndexType.PrimaryKey:
					new ConstraintDrop(Owner, Index).Execute();
					break;
			}
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"DROP INDEX {Index.Name} ON {Escape(Model.SchemaName(), Model.Name)};");

				return text.ToString();
			}
		}
	}
}