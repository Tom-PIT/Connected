namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal abstract class SynchronizationTransaction : SynchronizationCommand
	{
		public SynchronizationTransaction(ISynchronizer owner) : base(owner)
		{
		}

		public void Execute()
		{
			OnExecute();
		}

		protected virtual void OnExecute()
		{

		}

	}
}
