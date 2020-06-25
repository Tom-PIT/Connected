using System.Linq;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class PrimaryKeyRemove : ColumnTransaction
	{
		public PrimaryKeyRemove(ISynchronizer owner, ExistingModel existing, IModelSchemaColumn column) : base(owner, column)
		{
			Existing = existing;
		}

		private ExistingModel Existing { get; }

		protected override void OnExecute()
		{
			var constraint = Existing.Indexes.FirstOrDefault(f => f.Type == IndexType.PrimaryKey);

			new ConstraintDrop(Owner, constraint);

			Existing.Indexes.Remove(constraint);
		}
	}
}
