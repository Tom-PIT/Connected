using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace TomPIT.DataProviders.BigData
{
	internal class BigDataTransaction : DbTransaction
	{
		private List<BigDataCommand> _commits = null;
		public BigDataTransaction(BigDataConnection connection, IsolationLevel level)
		{
			DbConnection = connection;
			IsolationLevel = level;
		}
		public override IsolationLevel IsolationLevel { get; }

		protected override DbConnection DbConnection { get; }


		public override void Commit()
		{
			foreach (var command in Commits)
				command.Commit();
		}

		public override void Rollback()
		{
			foreach (var command in Commits)
				command.Cancel();
		}

		public void RegisterCommand(BigDataCommand command)
		{
			Commits.Add(command);
		}
		private List<BigDataCommand> Commits
		{
			get
			{
				if (_commits == null)
					_commits = new List<BigDataCommand>();

				return _commits;
			}
		}
	}
}
