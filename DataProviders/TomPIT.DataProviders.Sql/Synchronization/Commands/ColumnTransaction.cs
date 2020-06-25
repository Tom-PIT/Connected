using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal abstract class ColumnTransaction : TableTransaction
	{
		public ColumnTransaction(ISynchronizer owner, IModelSchemaColumn column) : base(owner)
		{
			Column = column;
		}

		protected IModelSchemaColumn Column { get; }
	}
}
