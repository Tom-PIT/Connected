using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace TomPIT.DataProviders.BigData
{
	internal class BigDataTransaction : DbTransaction
	{
		private readonly object _sync = new();
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
			foreach (var command in CreateBatches())
				command.Commit();
		}

		public override void Rollback()
		{
			foreach (var command in Snapshot())
				command.Cancel();
		}

		public void RegisterCommand(BigDataCommand command)
		{
			lock (_sync)
				Commits.Add(command);
		}
		/*
		 * Data providers create a new command for every execution, so each command holds only the rows
		 * of its own executions. Rows are merged per partition (CommandText) here so that a transaction
		 * performs a single write per partition regardless of how many commands produced the rows.
		 * Partition and row order is preserved.
		 */
		internal List<BigDataCommand> CreateBatches()
		{
			var result = new List<BigDataCommand>();

			foreach (var partition in Snapshot().GroupBy(f => f.CommandText, StringComparer.Ordinal))
			{
				var target = partition.First();

				foreach (var command in partition.Skip(1))
					target.Merge(command);

				result.Add(target);
			}

			return result;
		}

		private List<BigDataCommand> Snapshot()
		{
			lock (_sync)
				return new List<BigDataCommand>(Commits);
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
