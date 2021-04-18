namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal abstract class SynchronizationQuery<T> : SynchronizationCommand
	{
		public SynchronizationQuery(ISynchronizer owner) : base(owner)
		{
		}

		public T Execute()
		{
			return OnExecute();
		}

		protected virtual T OnExecute()
		{
			return default;
		}
	}
}
